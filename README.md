# Lueben.Microservice.Core

### Overview

**Lueben.Microservice.Core** is a comprehensive .NET Core solution designed to provide a set of reusable libraries, utilities, and components for building scalable, maintainable microservices and Azure Functions. This repository contains a collection of core shared projects that serve as the foundation for various microservices, APIs, and Azure Function-based projects within an enterprise-level architecture.

### Features

- **Reusability:** Centralized common functionality to ensure DRY (Don't Repeat Yourself) principles across microservices.
- **Scalability:** Designed with scalability in mind to support high-traffic environments and complex workflows.
- **Modularity:** Each microservice can be developed, tested, and deployed independently.
- **Serverless-Ready:** Seamless integration with Azure Functions for event-driven, serverless applications.
- **Monitoring and Logging:** Built-in support for Application Insights to monitor performance and diagnose issues in real-time.

### Getting Started

1. **Clone the repository:**
   ```bash
   git clone https://github.com/yourusername/Lueben.Microservice.Core.git
   ```
2. **Restore NuGet packages:**
   ```bash
   dotnet restore
   ```
3. **Build the solution:**
   ```bash
   dotnet build
   ```
4. **Run the tests (if applicable):**
   ```bash
   dotnet test
   ```

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

