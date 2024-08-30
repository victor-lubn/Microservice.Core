# Lueben.Microservice.Core

### Overview

**Lueben.Microservice.Core** is a comprehensive .NET Core solution designed to provide a set of reusable libraries, utilities, and components for building scalable, maintainable microservices and Azure Functions. This repository contains a collection of core shared projects that serve as the foundation for various microservices, APIs, and Azure Function-based projects within an enterprise-level architecture.

### Features

- **Reusability:** Centralized common functionality to ensure DRY (Don't Repeat Yourself) principles across microservices.
- **Scalability:** Designed with scalability in mind to support high-traffic environments and complex workflows.
- **Modularity:** Each microservice can be developed, tested, and deployed independently.
- **Serverless-Ready:** Seamless integration with Azure Functions for event-driven, serverless applications.
- **Monitoring and Logging:** Built-in support for Application Insights to monitor performance and diagnose issues in real-time.

# Nuget packages solution

These packages can be easily shared between different projects.
All the necessary metadata for all the Nuget packages is defined in _Nuget specification file_. The example of this file is listed below:

```
<?xml version="1.0"?>
<package>
    <metadata>
        <id>Lueben.Microservice.CircuitBreaker</id>
        <version>1.0.2</version>
        <title>Lueben.Microservice.CircuitBreaker</title>
        <authors>Lueben Joinery</authors>
        <description>Lueben.Microservice.CircuitBreaker</description>
        <contentFiles>
            <files include="cs/**/*.*" buildAction="Compile" /> 
        </contentFiles>
        <dependencies>
            <dependency id="Polly" version="7.2.1" />
            <dependency id="Polly.Caching.Memory" version="3.0.2" />
        </dependencies>
    </metadata>
    <files>
        <file src="bin\Release\netstandard2.1\*.dll" target="lib\any" />
        <file src="CircuitBreakerFunctions.cs" target="contentFiles\cs\any\CircuitBreaker" />
        <file src="DurableCircuitBreakerExternalApi.cs" target="contentFiles\cs\any\CircuitBreaker" />
    </files>
</package>
```
Apart from metadata of the Nuget package this Nuget specification file also stores the information about all the necessary dependencies that this Nuget package uses as well as the collection of the files that should be added to the project when this Nuget package is being installed.

The path to the Nuget specification file is defined in _csproj_ _file_ for all the projects inside our solution.

# Nuget Feed

All the packages are stored in the Nuget feed. For we use Azure DevOps _Artifacts_ as the main Nuget feed. 

This Nuget feed consists of these packages:
- _Lueben.Microservice.ApplicationInsights_ - the package that helps track events to Application Insights;
- _Lueben.Microservice.CircuitBreaker_ - the implementation of durable circuit braker pattern;
- _Lueben.Microservice.Mediator_ - the implementation of Mediator pattern;
- _Lueben.Microservice.OpenApi_ - the package that helps expose swagger documentation;
- _Lueben.Microservice.RetryPolicy_ - the implementation of retry pattern.

# Nuget config file

Because of the fact that in we are using a NuGet packages from a _private Azure DevOps package feed_, we cannot build the solution in Visual Studio because we are not authorized to access the feed.

For solutions with **NuGet.config** included it is enough to install Azure Artifacts Credential Provider from [this page](https://github.com/microsoft/artifacts-credprovider#azure-artifacts-credential-provider).

In order to get access to a private Azure DevOps package feed a **Personal Access Token** should be generated. All the steps that should be taken in order to generate Personal Access Token you can find in [this page](https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=azure-devops&tabs=preview-page)

The next step in order to be able to restore the NuGet package from the private feed is to add a **nuget.config** file to the root folder of the solution. The example of the nuget config file is the following:

```
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <packageSources>
        <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
        <add key="NameOfYourFeed" value="path to your nuget/index.json" />
    </packageSources>
</configuration>
```

After you've generated the PAT and created a Nuget config file, this token should be added to nuget config file. The Visual studio will grab this token from nuget config file while authenticating to a private Nuget Feed.

There are two ways to get it done:

## Adding PAT to local nuget config file:

You can add you PAT to nuget config file in section _packageSourceCredentials_. Here is the example of this section

```
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <!-- packageSources -->
    <packageSourceCredentials>
        <NameOfYourFeed>
            <add key="Username" value="gnabber"/>
            <add key="ClearTextPassword" value="YourPassword"/>
        </NameOfYourFeed>
    </packageSourceCredentials>
</configuration>
```

## Adding PAT to global nuget config file:

You can also add the generated PAT to global nuget config file that is placed at _%appdata%\Roaming\NuGet\NuGet.Config_. You need to take two steps to make it done:

   a. Go to the [NuGet page](https://www.nuget.org/downloads) and download nuget.exe.
   b. Register the private NuGet feed with the following command

`nuget.exe sources Add -Name "MyFeedName" -Source "https://myfeedurl" -username unused -password MyAccessToken`

---

### Set up a CI/CD pipeline
   
To set up a CI/CD pipeline in Azure DevOps that builds your C# libraries from a GitHub repository and publishes them to Azure DevOps Artifacts (NuGet feeds), follow these steps:

### **Step 1: Link GitHub Repository to Azure DevOps**

1. **Create a Project in Azure DevOps**:
   - Go to [Azure DevOps](https://dev.azure.com/).
   - Create a new project if you don’t have one already.

2. **Link the GitHub Repository**:
   - In your Azure DevOps project, navigate to **Project settings** > **Service connections**.
   - Create a new **GitHub** service connection by selecting **New service connection** > **GitHub**.
   - Authenticate and select your GitHub repository.

### **Step 2: Set Up the Build Pipeline**

1. **Create a New Build Pipeline**:
   - In Azure DevOps, go to **Pipelines** > **Create Pipeline**.
   - Choose **GitHub** as the source and select your repository.

2. **Select the Pipeline Configuration**:
   - If prompted, select the YAML option, which allows you to configure your pipeline as code.
   - You can also start with the classic editor for a more visual approach.

3. **Define the YAML Build Pipeline**:
   Create a `azure-pipelines.yml` file in your GitHub repository’s root directory, or edit it directly in Azure DevOps. Here’s an example configuration:

   ```yaml
   trigger:
     branches:
       include:
         - main  # Or the branch you want to trigger builds on

   pool:
     vmImage: 'windows-latest'

   steps:
   - task: UseDotNet@2
     inputs:
       packageType: 'sdk'
       version: '7.x'  # or the version of .NET SDK you need
       installationPath: $(Agent.ToolsDirectory)/dotnet

   - script: |
       dotnet restore
       dotnet build --configuration Release
     displayName: 'Restore and Build'

	- task: NuGetCommand@2
	  displayName: 'NuGet pack'
	  inputs:
		command: 'pack'
		packagesToPack: '**/*.csproj'  # or specify the .nuspec file if not using a .csproj
		configuration: 'Release'
		outputDir: '$(Build.ArtifactStagingDirectory)'

   - task: PublishBuildArtifacts@1
     inputs:
       PathtoPublish: '$(Build.ArtifactStagingDirectory)'
       ArtifactName: 'drop'
       publishLocation: 'Container'
   ```

4. **Save and Run**:
   - Save the pipeline and run it to ensure everything is set up correctly.

### **Step 3: Set Up the Release Pipeline**

1. **Create a New Release Pipeline**:
   - Go to **Pipelines** > **Releases**.
   - Click **New pipeline**.

2. **Add an Artifact**:
   - Select the build pipeline you created earlier as the source for the artifact.

3. **Define the Release Pipeline Stages**:
   - Click on **Add a stage** and select **Empty job**.
   - Name the stage (e.g., "Deploy to NuGet").

4. **Add a Job to Publish to Azure Artifacts**:
   - Click on **Tasks** under the stage you just created.
   - Click on the **+** icon to add a new task.

   **NuGet Push**:
   - **Push to Azure Artifacts**:
     - Add another task for **NuGet** using the "Push" command.
     - Configure it with your NuGet feed in Azure Artifacts.

     ```yaml
     - task: NuGetCommand@2
       inputs:
         command: 'push'
         packagesToPush: '$(Build.ArtifactStagingDirectory)/**/*.nupkg'
         nuGetFeedType: 'internal'
         publishVstsFeed: 'YourProjectName/YourFeedName'
     ```

5. **Configure Release Triggers**:
   - Set the release to trigger automatically upon successful builds.

6. **Save and Create a Release**:
   - Save the release pipeline and create a new release to test the entire process.

### **Step 4: Use the Published NuGet Packages**

After the release pipeline successfully runs, your NuGet packages will be available in the Azure Artifacts feed. You can add this feed to your projects in Visual Studio or via `NuGet.config` by following these steps:

1. **Add Azure Artifacts Feed to Visual Studio**:
   - In Visual Studio, go to **Tools** > **NuGet Package Manager** > **Package Manager Settings**.
   - Under **Package Sources**, add a new source with the feed URL from Azure Artifacts.

2. **Use the Packages**:
   - Now, you can reference your NuGet packages in your projects by searching for them in the NuGet Package Manager.

### **Summary**

1. **Set up a build pipeline** in Azure DevOps to build your C# libraries from the linked GitHub repository.
2. **Create a release pipeline** to package and publish the built DLLs as NuGet packages to an Azure Artifacts feed.
3. **Add the feed to your development environment** to consume the packages in your projects.

This setup provides a fully automated CI/CD pipeline for building and distributing your C# libraries via Azure DevOps.

### Contributing

We welcome contributions to improve the functionality and documentation of this repository.

### License

This project is licensed under the MIT License.

### Contact

For any questions or suggestions, please reach out to the repository owner or open an issue.

---

