<div align="center">
  <img height="200" src="https://github.com/user-attachments/assets/2451e9ab-119e-4378-9af2-feb0f5d0786b">  
</div>

----
# reFresh Core ![build status](https://github.com/utad-reFresh/core/actions/workflows/dotnet.yml/badge.svg)
O reFresh Core é uma aplicação ASP.NET que implementa a API e o website da plataforma, utilizando Blazor para uma experiência dinâmica e eficiente.

## appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=<<nome da base de dados>>;Username=<<username do postgres>>;Password=<<password do postgres>>"
  },
  "EmailSettings": {
    "SmtpServer": "<<url smtp.exemplo.pt>>",
    "SmtpPort": 587,
    "SmtpUsername": "<<username playable@exemplo.pt>>",
    "SmtpPassword": "<<password>>",
    "FromEmail": "<<email from playable@exemplo.pt>>",
    "FromName": "<<nome do email reFresh account services>>",
    "ApplicationUrl": "<<link da app https://playable.jestev.es>>"
  },
  "Jwt": {
    "Issuer": "<<issuer>>",
    "Audience": "<<audience>>",
    "Key": "<<key>>"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",

}
```
