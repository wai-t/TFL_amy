swagger file v1.json downloaded from https://api.tfl.gov.uk/swagger/docs/v1

You need to make sure you have nswag installed:
```
dotnet tool install --global NSwag.ConsoleCore
```

Define this pre-build event in this project's properties:
```
nswag openapi2csclient /input:swagger/v1.json /output:Generated/TflApi.cs /namespace:tfl__stats.Tfl
```
Add NewtonSoft.Json Nuget package to the project.
