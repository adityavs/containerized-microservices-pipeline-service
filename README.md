# Database
The Database folder container scripts and files required to develop on the database

## Run the database in a container
- (Optional) Create the  image `docker build -t login-backend .`. Make sure you are in the `Database` folder.
- Run the image in a Docker container: `docker run -p 1433:1433 --rm -it login-backend`

# Middle Tier - Login Service

## Run the Login Service in a container
- (Optional) Create the image `docker build -t login-middletier .`. Make sure you are in the `LoginService` folder.
- Run the image in a Docker container: `docker run -p 4201:4201 --rm -it login-middletier`.


## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
