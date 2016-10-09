# servicefabric-advanced
Sample project with advanced usage of Service Fabric.

## Description

This repository contains a Visual Studio solution that contains a sample application built on top of `Service Fabric`.

The goal of the entire project is that of providing a reference and (hopefully) good practices when creating a  
complex `Service Fabric` application using different components:

- `nginx` as reverse proxy
- `.NET core` for web application(s)
- `IdentityServer` for authentication
- `Redis` for caching
- `Serilog` for logging
- `cake` for building pipeline