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
   - Environment -> DEV  (Note if you are working with several people all sharing a resource group them make this DEV-{yourname})
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
   - Set the Source Slot: Preprod)   
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

### Add conditions
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

### Implement a Gate
1. Edit the Build 
2. Clone the Copy Files task SampleWebApp-ARM-Templates
3. Edit the new task and change the folder to AzureFunction-ARM-Templates
4. Add a new task Archive Files
   - Root Folder: AzureFunction-Code
   - Archive File to Create: $(Build.ArtifactStagingDirectory)/AzureFunction.zip
5. Save and Build
6. Edit the Prod stage
7. Clone the ARM task
   - Edit Template path (e.g. $(System.DefaultWorkingDirectory)/_Training-Azure-DevOps-Build/drop/arm-function-template.json)
   - Edit the Override template parameters 
      - NOTE: You might need to change the storage account name to be unique
     ```
     -AzureFunctionPlanName $(AppServicePlan)-Function-$(Environment) -AzureFunctionAppName $(WebApp)-Function-$(Environment)  -AzureFunctionStorageAccountName functionappstorage001
     ```
8. Clone the App Service Deploy task
   - Change App Type to Function App
   - Uncheck deploy to slot
   - Change the Package / Folder path (e.g. $(System.DefaultWorkingDirectory)/_Training-Azure-DevOps-Build/drop/AzureFunctionode.zip)
      - The code has been zipped up for your convience
9. Change the variable DeployARMTemplate to "true" (since we have a new ARM template). You can disable the approvals as well to save time.     
10. Save and run to make sure things are working, you should also check to make sure the Function App was deployed      
11. Change the variable DeployARMTemplate to "false" if things worked
12. Go to the Function App in the Azure Portal and copy the URL / security code
13. Click on the Lightning bolt of the Prod Swap stage
    - Enable a gate
    - Enter the function url (e.g. https://trainingazuredevopswebapp-function-prod.azurewebsites.net/api/AzureDevOpsFunctionGate)
    - Enter the code (e.g. kw99xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx=)
    - Under advanced for Success Criteria enter "eq(root['status'], 'true')"
    - NOTE: If you have an apporval you can check the box "On successfuly gates, ask for approvals", this means your gate will be evaluated first and must be successful before asking for an approval
    - You can change the evaluation times as well for testing
14. Save and Run a release
    - You can watch the gate perform its test
    


### To Do
Create Azure container registry
Create Azure Linux Web App
Build Docker
Push to container registry
Publish to Linux App

## Notes 
- If your web app gets a DLL locked, then add a task to restart the staging slot web app

- Some customers like to tear down the staging slot after deployments.  You can add this part of your pipeline.  I usually wait 1 to 2 hours before tearing down staging since if something goes wrong with the release your can quick swap slots.  You can add a step to perform the delete after a certain amount of time (e.g. Gate or an appoval with a deferred time). 

- My preprod staging slot is pointing to production resources and production databases

- If you are using Web Apps and use Slot specific variables be-aware that after your slot swap the appliction must recycle to load the values!  This negates most of the benefits of slots.  If your application has a quick warm up time for the first call, you might be alreay, but if not, your users can see a delay.

- If your web app is event based, for instance an Azure Function that triggers based upon a blob, then when you deploy to a the preprod staging slot, this function is processing production data!
   - How do you handle this?  For a blob trigger, if your preprod slot picks up the blob, it is not like it can pass the event to the production slot.  You also cannot have the production slot monitoring one container and preprod monitoring a different one.  
   - My perference for all event driven logic, have an Azure function that places items in a queue (or just place the item in the queue to begin with).  Then process the queue only if you are in the production slot.  You can test the URL of the application in some cases (not if you are directing 90% of your traffic the production slot and 10% to staging).  Having an application configuraiton value will not help since when you swap slots the value will not change.  I worry about missing events - especailly when new code is being deployed for an event based process.


## Best Practices
- I usually name my resource groups with a "-DEV", "-QA" and "-PROD".  Naming your items accordingly will make creating new environments easy.  Also, if you want to deploy to many Azure Regions see: https://github.com/AdamPaternostro/Azure-Dual-Region-Deployment-Approach

- The DevOps team and developers should work together to build the original pipeline.  Developers needs to understand what they need to expose as "variables" to the CI/CD engine.

- I have all my application setup for 24/7 deployments.  This means that I can deploy at anytime without the need for downtime.  The cloud makes this "easy" by providing the abilty to stand up the next version of your application, smoke test it and then swap to production.  My goal is to have the business users approve the releases to QA and Production.  To support this, along with CI/CD, please review: https://github.com/AdamPaternostro/Azure-Dev-Ops-Single-CI-CD-Dev-To-Production-Pipeline.  This shows how you can have a single pipeline from Dev to Prod while using CI/CD releases to Dev.


## Database DevOps (Schema Changes)
If you have a database as part of your application
1. You should deploy your database schema changes before deploying your application
2. Database schema changes should NOT break your current application
   - No dropping of columns, tables, stored procedures, etc.
   - If you add a column, it must have a default value
   - If you add a parameter to a stored procedure, it must have a default value
3. When you need to drop an item (e.g. Table)   
   - If your current code release is version 1.0 and then next release is version 1.1
   - Do not drop the table in v1.1 deployment
   - When you deploy your v1.1 code to the preprod slot the production slot will have v1.0, so BOTH v1.0 and v1.1 will be running and using the same database schema. 
   - Drop the table in the v1.2 release since v1.1 will be in the production slot and v1.2 will go to preprod and netiher release relies on the dropped table
4. If you have seed data (e.g. states table or zipcode table, etc.)   
   - Create a stored procedure called "InitializeDatabase"
   - This procedure should be very robust/idempotent
      - IF NOT EXISTS(SELECT 1 FROM myTABLE WHERE id = 10) THEN INSERT...
   - When your code starts up (or part of Dev Ops), you should call this procedure
   - By placing all your look up values in a stored procedure, the stored procedure should be under source control (yes, you should be using database source control)


## Database DevOps (Data Updates)
- For minor data updates you can run these as part of the "InitializeDataUpdate" stored prodcedure much like the "InitializeDatabase".  This procedure can do tests like, if all values in a column is NULL then seed the value.
- For large data changes, like changing all the values of a lookup table used in calcuations, you can backup the table and then update the entire table.  If this is millions of rows and takes a long time, you can backup the database (the cloud does this for you), then issue the update.  I currently do not have a good DevOps way of hanlding this.  It is mainly manual since some updates might take hours.  Rolling back is hard since you might be looking at a restore and copy data.


## Azure Resource DevOps
- With the cloud, your code should create all the items within a resource.
- Your infrastructure as code should create the Azure Storage account
- Your code should create all the blob containers, queues, tables, etc. 
- The goal of your code should be that you can compile and run without any prerequisites!  This way you can spin up new environments quickly and without a person creating a bunch of required dependencies.


## Beware of ourside shared resources
- If you code uses a file in blob storage, lets say it if a PDF file with fields that need to be populate by your code
- If you need to update this file and are using the staging slot technique then when you deploy to staging, this file will mostlikely be updated.  The issue is the current production code is now using an updated file.
- Try to keep these dependecies as part of your project


## Configuration values
- I have mainly given up on updating a application configuration value when an application is running in production.  You should have some configuration values as part of the application settings (e.g. environment variables).  If you need to update a value, I perfer to push out a whole new release.  
- My code typically loads the configuration values at runtime.  The code does not get these values from application settings.  The code loads the values from something like KeyVault.  I usually have my configuration values named "DatabaseConnectionString-{Environment}" and I get the ENVIRONMENT variable from my settings and then load based upon the string.  This means my code is really dependent on just a single application setting variable named ENVIRONMENT. This avoids having all the configuration values as part of my Dev Ops process.



## RDP / SSH access
- If you do your DevOps process correctly you should never allow RDP or SSH access to a machine in QA or Production. Dev is okay for troubleshoot some items, but I have not remoted to a production machine in many years.


## How I do DevOps on my projects
1. Create a resource group named MyProject-PoC (this is my playground)
2. Create my Azure resources by hand and do some testing (change / delete resources)
3. Create a Hello World app that has all my tiers (make sure the App works)
4. Add security to my Hello World app to all my tiers (pass security between tiers)
5. Export my ARM template from the Azure Portal Edit my ARM template.  Create parameters for everything.
6. Run my ARM template and create a new resource group called MyProject-DEV (all my resources will have a –DEV suffix)
7. Run my application and make sure it works just like step 4. 
8. Repeat Steps 6 and 7 over and over!
9. The code and ARM template should now be in source control
10. Create a build definition
11. Create a release definition
12. Run it.  Make sure it works.  
13. Delete my MyProject-POC since everything should now be automated.
14. Create a QA and Prod pipeline by cloning the Dev pipeline (they should use a suffix of –QA and –PROD)
15. Now code!
16. Implement Logging, Error handling and Monitoring
17. Make minor adjustments to my CI/CD pipeline.


