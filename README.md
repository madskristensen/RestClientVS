# Rest Client for Visual Studio

[![Build status](https://ci.appveyor.com/api/projects/status/p149td5c23fap553?svg=true)](https://ci.appveyor.com/project/madskristensen/restclientvs)

Download this extension from the [VS Marketplace](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.RestClient)
or get the [CI build](https://www.vsixgallery.com/extension/RestClientVS.a7b4a362-3ce8-4953-9b19-a35166f2cbfd).

--------------------------------

REST Client allows you to send HTTP request and view the response in Visual Studio directly.. Based on the popular VS Code extension [Rest Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) by [Huachao Mao](https://github.com/Huachao)

## The .http file
Any files with the extension `.http` is the entry point to creating HTTP requests.

Here's an example of how to define the requests with variables and code comments.

```css
@hostname = localhost
@port = 8080
@host = {{hostname}}:{{port}}
@contentType = application/json

POST https://{{host}}/authors/create
Content-Type:{{contentType}}

{
    "name": "Joe",
    "permissions": "author"
}

###

# Comments
GET https://{{host}}/authors/

###

GET https://www.bing.com
```

This is what it looks like in the Blue Theme.

![Document](art/document.png)

Notice the green play buttons showing up after each URL. Clicking those will fire off the request and display the raw response in the Output Widow.

![Output](art/output.png)

Peek at the value of the variables by moving the mouse over any line containing a variable reference.

![Tooltip](art/tooltip.png)

You can also right-click to find the Send Request command or use the **Ctrl+Alt+S** keyboard shortcut.

![Context Menu](art/context-menu.png)

You can set the timeout of the HTTP requests from the *Options* dialog.

![Options](art/options.png)

## License
[Apache 2.0](LICENSE) 
