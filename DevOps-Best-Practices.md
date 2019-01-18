# Overview
Here are the items I have learned and implement on my DevOps in Azure.  

# Best Practices
- I usually name my resource groups with a "-DEV", "-QA" and "-PROD".  Naming your items accordingly will make creating new environments easy.  Also, if you want to deploy to many Azure Regions see: https://github.com/AdamPaternostro/Azure-Dual-Region-Deployment-Approach

- The DevOps team and developers should work together to build the original pipeline.  Developers needs to understand what they need to expose as "variables" to the CI/CD engine.

- I have all my applications setup for 24/7 deployments.  This means that I can deploy at anytime without the need for downtime.  The cloud makes this "easy" by providing the abilty to stand up the next version of your application, smoke test it and then swap to production.  My goal is to have the business users approve the releases to QA and Production.  To support this, along with CI/CD, please review: https://github.com/AdamPaternostro/Azure-Dev-Ops-Single-CI-CD-Dev-To-Production-Pipeline.  This shows how you can have a single pipeline from Dev to Prod while using CI/CD releases to Dev.


# Database DevOps (Schema Changes)
If you have a database as part of your application
1. You should deploy your database schema changes before deploying your application.  A seperate deployment.
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
      - IF NOT EXISTS(SELECT 1 FROM myTABLE WHERE id = 10) THEN INSERT INTO myTable (myField) VALUES (10);
   - When your code starts up (or part of Dev Ops), you should call this procedure
   - By placing all your look up values in a stored procedure, the stored procedure should be under source control (yes, you should be using database source control)


# Database DevOps (Data Updates)
- For minor data updates you can run these as part of a "InitializeDataUpdate" stored procedure much like the "InitializeDatabase".  This procedure can do tests like, if all values in a column is NULL then seed the value.
- For large data changes, like changing all the values of a lookup table used in calcuations, you can backup the table and then update the entire table.  If this is millions of rows and takes a long time, you can backup the database (the cloud does this for you), then issue the update.  I currently do not have a good DevOps way of hanlding this.  It is mainly manual since some updates might take hours.  Rolling back is hard since you might be looking at a restore and copy data.  


# Azure Resource DevOps
- With the cloud, your code should create all the items within a resource.
   - Your infrastructure as code should create the Azure Storage account
   - Your code should create all the blob containers, queues, tables, etc. 
- The goal of your code should be that you can compile and run without any prerequisites!  This way you can spin up new environments quickly and without a person creating a bunch of required dependencies.


# Beware of ourside shared resources
- If you code uses a file in blob storage, lets say it if a PDF file with fields that need to be populate by your code
- If you need to update this file and are using the staging slot technique then when you deploy to staging, this file will mostlikely be updated.  The issue is the current production code is now using an updated file.
- Try to keep these dependecies as part of your project


# Configuration values
- I have mainly given up on updating an application configuration value when an application is running in production.  You should have some configuration values as part of the application settings (e.g. environment variables).  If you need to update a value, I perfer to push out a whole new release.  
- My code typically loads the configuration values at runtime.  The code does not get these values from application settings.  The code loads the values from something like KeyVault.  I usually have my configuration values named "DatabaseConnectionString-{Environment}" and I get the ENVIRONMENT variable from my settings / DevOps and then load based upon the string.  This means my code is really dependent on just a single application setting variable named ENVIRONMENT. This avoids having all the configuration/secret values as part of my Dev Ops process.  If someone wants to change a database password, then they change it in KeyVault.  You application should cache the values from KeyVault for an acceptable amount of time (e.g. 5 minutes).  This way if a value changes, you should be able to do your key rotations in 5 to 10 minute window.


# RDP / SSH access
- If you do your DevOps process correctly you should never allow RDP or SSH access to a machine in QA or Production. Dev is okay for troubleshoot some items (remote installs), but I have not remoted to a production machine in many, many, many years.


# How I do DevOps on my projects
1. Create a resource group named MyProject-PoC (this is my playground)
2. Create my Azure resources by hand and do some testing (change / delete resources)
3. Create a Hello World app that has all my tiers (make sure the App works)
4. Add security to my Hello World app to all my tiers (pass security between tiers and get the security teams sign-off)
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