var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var gatewayDir = Directory("./src/Gateway");
var gatewayOutputDir = gatewayDir + Directory(configuration);

var identityServerDir = Directory("./src/IdentityServer");
var identityserverOutputDir = identityServerDir + Directory(configuration);

var webApplicationDir = Directory("./src/WebApplication");
var webApplicationOutputDir = webApplicationDir + Directory(configuration);

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(gatewayOutputDir);
    CleanDirectory(identityserverOutputDir);
    CleanDirectory(webApplicationOutputDir);
});

Task("Restore")
    .Does(() =>
{
    DotNetCoreRestore(gatewayDir.ToString());
    DotNetCoreRestore(identityServerDir.ToString());
    DotNetCoreRestore(webApplicationDir.ToString());
});

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
{
    DotNetCoreBuild(gatewayDir.ToString());
    DotNetCoreBuild(identityServerDir.ToString());
    DotNetCoreBuild(webApplicationDir.ToString());
});

Task("Rebuild")
    .IsDependentOn("Clean")
    .IsDependentOn("Build")
    .Does(() =>
{
});

Task("Default")
    .IsDependentOn("Rebuild")
  .Does(() =>
{
  Information("Hello World!");
});

RunTarget(target);