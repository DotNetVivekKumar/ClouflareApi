# Cloudflare IP Blocker

This ASP.NET MVC web application utilizes Cloudflare's APIs to manage IP addresses in the Web Application Firewall (WAF) IP Access Rules. The main functionality includes checking if specified IP addresses are already blocked or allowed in the firewall rules. If an IP address is found to be allowed, the application updates the rule to block it instead. If the IP address is already blocked, the application returns a message indicating that the IP is already blocked. If the IP address is not found in the rules, the application blocks the given IP address in Cloudflare.

## Features

- **Block IP Addresses**: The application blocks multiple IP addresses using Cloudflare's API.
- **Update IP Rules**: If an IP address is found to be allowed, the application updates the rule to block it.
- **Check IP Status**: The application checks if specified IP addresses are already blocked or allowed in the firewall rules.
- **Error Handling**: The application handles errors gracefully and provides appropriate error messages.
- **User Interface**: Simple and intuitive user interface to enter IP addresses and view status messages.

## Technologies Used

- **ASP.NET MVC**: The web application is built using the ASP.NET MVC framework.
- **C#**: Backend logic and API integrations are implemented using C#.
- **HttpClient**: Used to send HTTP requests to Cloudflare's APIs.
- **Cloudflare API**: Utilized Cloudflare's GET, POST, and PATCH APIs to manage IP addresses in the firewall rules.

## Setup

1. Clone the repository to your local machine.
2. Open the solution in Visual Studio.
3. Update the `apiKey`, `email`, and `zoneId` variables in the `HomeController.cs` file with your Cloudflare credentials.
4. Build and run the application.

## Usage

1. Navigate to the IP Addresses page.
2. Enter the IP addresses you want to block (separated by commas) in the input field.
3. Submit the form.
4. View the success or error messages displayed on the page.

## Contributors

- [Vivek Kumar](https://github.com/DotNetVivekKumar)
