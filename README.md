# Training-Azure-DevOps
Using Azure Dev Ops to deploy Infrastructure as Code, Web application, Staging Slots and Docker images.


### Azure Setup
1. Create a resource group in Azure named: Training-Azure-DevOps
2. Create a service principle in Azure named (web/api): Training-Azure-DevOps-SP
3. Grant the service principle contributor access to the resource group

### Azure DevOps Setup
1. Create an Azure DevOps project named: Training-Azure-DevOps

### Open Azure DevOps
1. Click on Repos | Files
2. Click on Import under "or import a Repository"
3. Enter: https://github.com/AdamPaternostro/Training-Azure-DevOps.git

### Create a build defination
We will be using the visual interface
1. Click on Pipelines | Builds
2. Click on New Pipeline button
3. Click on the small link "Use the visual designed"
4. Click on Azure Repos Git (should all be filled out) | Click Continue
5. Select template "ASP.NET Core"
6. Click Save & Queue (make sure it works)
7. Click on the Artifacts button.  You should see a zip.

### Alter build defination
1. Add a Copy File task (click +)
2. Move it abovethe Publish Artifact task
2. Select source folder: SampleWebApp-ARM-Templates
3. Set target folder: $(build.artifactstagingdirectory)
4. Build again
5. View artifacts and you should see arm-template.json in the artifacts

### Create a Release pipeline (Deploy ARM Template)
1. Click on Pipelines | Releases | New Pipeline button
2. Click on Empty Job link (where it says Select a template)
3. Rename Stage 1 to Dev and click the X to close
4. Click on Add an Artifact
5. Select your build
6. Select Default version: Latest
7. Click on Variables at the top
8. Add variables
   - Environment -> DEV
   - AppServicePlan -> TrainingAzureDevOpsAppServicePlan (you can specify what you like - must be globally unique in Azure)
   - WebApp -> TrainingAzureDevOpsWebApp (you can specify what you like - must be globally unique in Azure)
   - ResourceGroup -> Training-Azure-DevOps
9. Click on "New release pipeline" and change the name
10. Click Save
11. Click on Dev Stage
12. Add a task (click +)
13. Select "Azure Resource Group Deployment"
14. Click on tasks
   - Click the manage link next to "Azure Subscription"
   - Click on New Service Connection
   - Click on Azure Resource Manager
   - Click on User full version of this dialog (link at bottom)
   - Give it a name
   - Enter the Service Principle Id (Application Id)
   - Enter the Key
   - Test the connection
   - If you get an error make sure the service principle exists and has Contributor access to your resource group
   - Close the tab and return to your pipeline
   - Click the refresh button next to Azure Subscription
   - Select the connection you just setup
   - Enter the resource group name: $(ResourceGroup)  (Typically I append the Environment name "DEV" to it, but for most enterprises resource groups are created by IT)
   - Pick your location (I keep mine the same as my resource group)
   - Pick your template "arm-template.json"
   - Leave parameters blank, we do not have a parameters file
   - In "Override template parameters" enter: 
     -serverfarms_TrainingAzureDevOpsAppServicePlan_name $(AppServicePlan)-$(Environment) -sites_TrainingAzureDevOpsWebApp_name $(WebApp)-$(Environment)  
   - Leave Deployment Mode as Incremental (this is very important as Complete will remove unused resources which can do things like delete a storage account)
15. Save
16. Click on Release | Create a Release
   - You can click the Release-1 link to view the release
   - You can view the resources being created in the Azure Portal

### Release pipeline (Deploy Code)
1. Edit the release pipeline
2. Add a task to the Dev stage "Azure App Service Deploy"
3. Edit the settings
   - Set the subscription
   - Set App Service name to $(WebApp)-$(Environment)
   - Set the Package / Folder to the SampleWebApp.zip file (use the selector)
   - Set the App Settings to "-Environment $(Environment)" under Application and Configuration Settings
4. Disable the ARM deployment task (right click and disable). This is a time saver during development.
5. Save and Run a release
6. After the release open the website (e.g. https://trainingazuredevopswebapp-dev.azurewebsites.net/)

### Release pipeline (Deploy to slot)
1. Edit the release pipeline
2. Edit the Web App Deployment
3. Click Deploy to Slot
   - Set resource group: $(ResourceGroup)
   - Set Slot to: Proprod
4. Save and Run a release
5. After the release open the websites 
   - e.g. https://trainingazuredevopswebapp-dev.azurewebsites.net/
   - e.g. https://trainingazuredevopswebapp-dev-PREPROD.azurewebsites.net/
6. You will notice the ServerName is the same (both apps are on the same server)

### Release pipeline (Swap slot)
1. Edit the release pipeline
2. Add a task to the Dev stage "Azure App Service Manage"
3. Edit the settings
   - Set the subscription
   - Set the App Service name: $(WebApp)-$(Environment)
   - Set the Resource Group: $(ResourceGroup)
   - Set the Source Slot: Preprod
4. Save and Run a release

### Create QA Release
1. Edit the release pipeline
2. Edit variables
3. Change Environment Scope to DEV
4. Click on Pipeline
5. Clone the Dev Stage
6. Click on new Stage and rename to QA
7. Click on variables, you will see Environment twice
8. Change the QA scope to "QA" for the value
9. Click on the QA stage and re-enable the ARM task
10. Save and Run a release
   - You can quickly make new environments onces you have your Dev pipeline working

### Create Production Release
1. Edit the release pipeline
2. Clone QA
3. Rename to Prod
4. Edit variables
5. In QA disable the ARM task
6. Clone Prod
7. Rename to Prod-Swap and Edit
8. Delete the ARM task from Prod-Swap
9. Delete the Web Deploy taks from Prod-Swap
10. Edit the Prod stage
11. Delete the Swap Slots task
12. Save and Run a release

### Add condtions
1. Added a variable called DeployARMTemplate and set the value to false
2. For each stage
   - Enable the ARM template
   - Edit the step
   - Under Control Options
   - Set Run this task to Custom Conditions
   - Set Custom condition: eq(variables['DeployARMTemplate'], 'true')
3. Save and Run a release
   - You should see the ARM template skipped

### Add approvals
1. Edit the release pipeline
2. Click on the person icon on the right side of the Dev stage
3. Enable "Post-deployment approvals"
4. Enter your name
5. Click on the person icon on the right side of the QA stage
6. Enable "Post-deployment approvals"
7. Enter your name
8. Save and Run a release
   You can approve on https://dev.azure.com or click on the email


### Change the code and enable automatic builds/releases
1. Edit the Build defination
2. Click on Triggers
3. Click the "Enable continuous integration" checkbox
4. Save the Build (do not Save and queue)
5. Edit the Release pipeline
6. Click on the Lightning bolt icon on the Artifacts
7. Enable "Continuous deployment trigger"
8. Save the Release
9. Click on Repo | Files
10. Edit the file SampleWebApp/Views/Home/Index.cshtml
11. At the bottom enter
   ```
   <div id="row">
    <div class="col-md-12">
        New Release
    </div>
   </div>
   ```
12. Save the file
   - The build should kick off automatically (click on bulids)
   - The release should kick off automatically (click on releases when build is done)
   - You can verify the new code is moving through environments by viewing the websites (the prod slot witll have the changes,the old site will be in the preprod slot)

### Do QA in parallel (just to demo)

Create Azure function
Create gate


Create Azure container registry
Create Azure Linux Web App
Import code
Build Docker
Push to container registry
Publish to Linux App


## Notes / Best Practices
- I usually name my resource groups with a "-DEV", "-QA" and "-PROD".  Naming your items accordingly will make creating new environments easy.  Also, if you want to deploy to many Azure Regions see: https://github.com/AdamPaternostro/Azure-Dual-Region-Deployment-Approach

- The DevOps team and developers should work together to build the original pipeline.  Developers needs to understand what they need to expose as "variables" to the CI/CD engine.

## How I do DevOps on my projects
1. Create a resource group named MyProject-PoC (this is my playground)
2. Create my Azure resources by hand and do some testing (change / delete resources)
3. Create a Hello World app that has all my tiers (make sure the App works)
Add security to my Hello World app to all my tiers (pass security between tiers)
4. Export my ARM template from the Azure Portal
5. Edit my ARM template.  Create parameters for everything.
6. Run my ARM template and create a new resource group called MyProject-DEV (all my resources will have a –DEV suffix)
7. Run my application and make sure it works just like step 4. Repeat Steps 6, 7 and 8 over and over!
8. The code and ARM template should now be in source control
9. Create a build definition
10. Create a release definition
11. Run it.  Make sure it works.  Repeat steps 10, 11 and 12 over and over!
12. Delete my MyProject-POC since everything should now be automated.
13. Create a QA and Prod pipeline by cloning the Dev pipeline (they should use a suffix of –QA and –PROD)
14. Now code!
15. Implement Logging, Error handling and Monitoring
16. Make minor adjustments to my CI/CD pipeline.


