# Description

This package contains functionality to support OAuth and function key authorization.

## Confidential Client
Confidential client should be used as singleton to benifit from token cachings.
Because of this it does not support refreshing of option parameters used to get a token.
So if "ClientId", "ClientSecret" or "Tenant" are changed then restart of an application is required to apply the changes.
