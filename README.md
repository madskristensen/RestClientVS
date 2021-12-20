# Rest Client for Visual Studio

[![Build](https://github.com/madskristensen/RestClientVS/actions/workflows/pr_build.yaml/badge.svg)](https://github.com/madskristensen/RestClientVS/actions/workflows/pr_build.yaml)
[![VS Marketplace](https://vsmarketplacebadges.dev/version-short/madskristensen.RestClient.svg)](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.RestClient)
![Installs](https://img.shields.io/visual-studio-marketplace/i/madskristensen.RestClient?label=Installs&logo=visualstudio)
![Rating](https://vsmarketplacebadges.dev/rating-short/madskristensen.RestClient.svg)

REST Client allows you to send HTTP request and view the response in Visual Studio directly. Based on the popular VS Code extension [Rest Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) by [Huachao Mao](https://github.com/Huachao)

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