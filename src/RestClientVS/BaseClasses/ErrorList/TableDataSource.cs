using System.Collections.Generic;
using System.Linq;
using Microsoft;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;

namespace BaseClasses
{
    public class TableDataSource : ITableDataSource
    {
        public TableDataSource(string identifier, string displayName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Identifier = identifier;
            DisplayName = displayName;
            Initialize();
        }

        /// <summary>
        /// Data sink subscriptions
        /// </summary>
        private readonly List<SinkManager> _managers = new();

        /// <summary>
        /// Error list snapshots
        /// </summary>
        private readonly Dictionary<string, TableEntriesSnapshot> _snapshots = new();

        /// <summary>
        /// 'Error list' columns/components exposed by items managed by this data source
        /// </summary>
        public virtual IReadOnlyCollection<string> Columns { get; } = new[]
        {
            StandardTableColumnDefinitions.DetailsExpander,
            StandardTableColumnDefinitions.ErrorCategory,
            StandardTableColumnDefinitions.ErrorSeverity,
            StandardTableColumnDefinitions.ErrorCode,
            StandardTableColumnDefinitions.ErrorSource,
            StandardTableColumnDefinitions.BuildTool,
            StandardTableColumnDefinitions.Text,
            StandardTableColumnDefinitions.DocumentName,
            StandardTableColumnDefinitions.Line,
            StandardTableColumnDefinitions.Column
        };

        public string SourceTypeIdentifier => StandardTableDataSources.ErrorTableDataSource;

        public virtual string Identifier { get; }

        public virtual string DisplayName { get; }

        public IDisposable Subscribe(ITableDataSink sink)
        {
            var manager = new SinkManager(sink, RemoveSinkManager);

            AddSinkManager(manager);

            return manager;
        }

        protected void Initialize()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var compositionService = ServiceProvider.GlobalProvider.GetService(typeof(SComponentModel)) as IComponentModel;
            Assumes.Present(compositionService);

            ITableManagerProvider tableManagerProvider = compositionService.GetService<ITableManagerProvider>();
            Assumes.Present(tableManagerProvider);

            ITableManager manager = tableManagerProvider.GetTableManager(StandardTables.ErrorsTable);
            manager.AddSource(this, Columns);
        }

        /// <summary>
        /// Registers a sink subscription
        /// </summary>
        /// <param name="manager">Subscription to register</param>
        private void AddSinkManager(SinkManager manager)
        {
            lock (_managers)
            {
                _managers.Add(manager);
            }
        }

        /// <summary>
        /// Unregisters a previously registered sink subscription
        /// </summary>
        /// <param name="manager">Subscription to unregister</param>
        private void RemoveSinkManager(SinkManager manager)
        {
            lock (_managers)
            {
                _managers.Remove(manager);
            }
        }

        /// <summary>
        /// Notifies all subscribers of an update in error (listings)
        /// </summary>
        private void UpdateAllSinks()
        {
            lock (_managers)
            {
                foreach (SinkManager manager in _managers)
                {
                    manager.UpdateSink(_snapshots.Values);
                }
            }
        }

        public void AddErrors(string projectName, IEnumerable<ErrorListItem> errors)
        {
            if (errors == null || !errors.Any())
            {
                return;
            }

            IEnumerable<ErrorListItem> cleanErrors = errors.Where(e => e != null && !string.IsNullOrEmpty(e.FileName));

            lock (_snapshots)
            {
                foreach (IGrouping<string, ErrorListItem> error in cleanErrors.GroupBy(e => e.FileName))
                {
                    _snapshots[error.Key] = new TableEntriesSnapshot(projectName, error.Key, error);
                }
            }

            UpdateAllSinks();
        }

        /// <summary>
        /// Clears all previously registered issues/errors
        /// </summary>
        public void CleanAllErrors()
        {
            lock (_snapshots)
            {
                lock (_managers)
                {
                    foreach (SinkManager manager in _managers)
                    {
                        manager.Clear();
                    }
                }

                foreach (TableEntriesSnapshot snapshot in _snapshots.Values)
                {
                    snapshot.Dispose();
                }

                _snapshots.Clear();
            }
        }
    }
}
