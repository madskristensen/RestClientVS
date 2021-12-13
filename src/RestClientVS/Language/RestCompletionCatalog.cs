using System.Collections.Generic;

namespace RestClientVS.Completion
{
    public class RestCompletionCatalog
    {
        public static IDictionary<string, string> HttpMethods = new Dictionary<string, string>()
        {
            {"GET",     "The GET method is used to retrieve information from the given server using a given URI.Requests using GET should only retrieve data and should have no other effect on the data."},
            {"HEAD",    "Same as GET, but transfers the status line and header section only."},
            {"POST",    "A POST request is used to send data to the server, for example, customer information, file upload, etc. using HTML forms"},
            {"PUT",     "Replaces all current representations of the target resource with the uploaded content."},
            {"DELETE",  "Removes all current representations of the target resource given by a URI."},
            {"TRACE",   "Performs a message loop-back test along the path to the target resource."},
            {"OPTIONS",  "Describes the communication options for the target resource."},
        };

        public static IDictionary<string, string> HttpHeaderNames = new Dictionary<string, string>()
        {
            {"A-IM","Acceptable instance-manipulations for the request."},
            {"Accept","Media type(s) that is/are acceptable for the response. See Content negotiation."},
            {"Accept-Charset","Character sets that are acceptable."},
            {"Accept-Datetime","Acceptable version in time."},
            {"Accept-Encoding","List of acceptable encodings. See HTTP compression."},
            {"Accept-Language","List of acceptable human languages for response. See Content negotiation."},
            {"Access-Control-Request-Method,","Initiates a request for cross-origin resource sharing with Origin (below)."},
            {"Access-Control-Request-Headers",""},
            {"Authorization","Authentication credentials for HTTP authentication."},
            {"Cache-Control","Used to specify directives that must be obeyed by all caching mechanisms along the request-response chain."},
            {"Connection","Control options for the current connection and list of hop-by-hop request fields."},
            {"Content-Encoding","The type of encoding used on the data. See HTTP compression."},
            {"Content-Length","The length of the request body in octets (8-bit bytes)."},
            {"Content-MD5","A Base64-encoded binary MD5 sum of the content of the request body."},
            {"Content-Type","The Media type of the body of the request (used with POST and PUT requests)."},
            {"Cookie","An HTTP cookie previously sent by the server with Set-Cookie (below)."},
            {"Date","The date and time at which the message was originated (in HTTP-date format as defined by RFC 7231 Date/Time Formats)."},
            {"Expect","Indicates that particular server behaviors are required by the client."},
            {"Forwarded","Disclose original information of a client connecting to a web server through an HTTP proxy."},
            {"From","The email address of the user making the request."},
            {"Host","The domain name of the server (for virtual hosting), and the TCP port number on which the server is listening. The port number may be omitted if the port is the standard port for the service requested."},
            {"HTTP2-Settings","A request that upgrades from HTTP/1.1 to HTTP/2 MUST include exactly one HTTP2-Setting header field. The HTTP2-Settings header field is a connection-specific header field that includes parameters that govern the HTTP/2 connection, provided in anticipation of the server accepting the request to upgrade."},
            {"If-Match","Only perform the action if the client supplied entity matches the same entity on the server. This is mainly for methods like PUT to only update a resource if it has not been modified since the user last updated it."},
            {"If-Modified-Since","Allows a 304 Not Modified to be returned if content is unchanged."},
            {"If-None-Match","Allows a 304 Not Modified to be returned if content is unchanged, see HTTP ETag."},
            {"If-Range","If the entity is unchanged, send me the part(s) that I am missing; otherwise, send me the entire new entity."},
            {"If-Unmodified-Since","Only send the response if the entity has not been modified since a specific time."},
            {"Max-Forwards","Limit the number of times the message can be forwarded through proxies or gateways."},
            {"Origin","Initiates a request for cross-origin resource sharing (asks server for Access-Control-* response fields)."},
            {"Pragma","Implementation-specific fields that may have various effects anywhere along the request-response chain."},
            {"Prefer","Allows client to request that certain behaviors be employed by a server while processing a request."},
            {"Proxy-Authorization","Authorization credentials for connecting to a proxy."},
            {"Range","Request only part of an entity. Bytes are numbered from 0. See Byte serving."},
            {"Referer","This is the address of the previous web page from which a link to the currently requested page was followed. (The word 'referrer' has been misspelled in the RFC as well as in most implementations to the point that it has become standard usage and is considered correct terminology)"},
            {"TE","The transfer encodings the user agent is willing to accept: the same values as for the response header field Transfer-Encoding can be used, plus the 'trailers' value (related to the 'chunked' transfer method) to notify the server it expects to receive additional fields in the trailer after the last, zero-sized, chunk."},
            {"Trailer","The Trailer general field value indicates that the given set of header fields is present in the trailer of a message encoded with chunked transfer coding."},
            {"Transfer-Encoding","The form of encoding used to safely transfer the entity to the user. Currently defined methods are: chunked, compress, deflate, gzip, identity."},
            {"User-Agent","The user agent string of the user agent."},
            {"Upgrade","Ask the server to upgrade to another protocol."},
            {"Via","Informs the server of proxies through which the request was sent."},
            {"Warning","A general warning about possible problems with the entity body."},
        };
    }
}
