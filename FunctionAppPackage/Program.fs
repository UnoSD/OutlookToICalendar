module Program

open Pulumi.FSharp.AzureNative.Components.FunctionAppPackage
open Pulumi.FSharp.NamingConventions.Azure.Resource
open Pulumi.FSharp.AzureNative.Logic.Inputs
open Pulumi.FSharp.AzureNative.Web.Inputs
open Pulumi.FSharp.AzureNative.Resources
open Pulumi.AzureNative.Authorization
open Pulumi.FSharp.AzureNative.Logic
open Pulumi.FSharp.AzureNative.Web
open Pulumi.AzureNative.Web
open Pulumi.FSharp.Outputs
open Pulumi.FSharp.Config
open Pulumi.FSharp
open System.IO
open Pulumi

Deployment.run (fun () ->
    let subId =
        output {
            let! result = GetClientConfig.InvokeAsync()

            return result.SubscriptionId
        }

    let workload, calendarId, email =
        config.["workloadOrApplication"],
        config.["calendarId"],
        config.["email"]


    let rg =
        resourceGroup {
            name (nameOne "rg")
        }

    let functionAppInfrastructure =
        functionAppPackage {
            workloadName              workload
            resourceGroupName         rg.Name
            projectPackagePublishPath config.["projectPublishPath"]
        }
    
    let office365Connection =
        connection {
            name          (nameOne "wcn")
            resourceGroup rg.Name

            apiConnectionDefinitionProperties {
                displayName email
                
                apiReference {
                    name         "office365"
                    resourceType "Microsoft.Web/locations/managedApis"
                    id           (Output.Format($"/subscriptions/{subId}/providers/Microsoft.Web/locations/westeurope/managedApis/office365"))
                }
            }
        }

    let parametersJson =
        output {
            let! connectionId =
                office365Connection.Id

            let! location = 
                office365Connection.Location

            let! subId =
                subId

            return File.ReadAllText("LogicApp.parameters.json")
                       .Replace("{connectionId}", connectionId)        
                       .Replace("{subscriptionId}", subId)        
                       .Replace("{location}", location)        
        }

    let logicAppJson =
        output {
            let! functionId =
                functionAppInfrastructure.App.Id

            let! functionName =
                functionAppInfrastructure.App.Name

            let! rgName =
                rg.Name

            let! result =
                ListWebAppHostKeys.InvokeAsync(ListWebAppHostKeysArgs(
                    Name = functionName,
                    ResourceGroupName = rgName
                ))

            return File.ReadAllText("LogicApp.json")
                       .Replace("{functionId}", functionId + "/functions/GetICal")
                       .Replace("{functionKey}", result.MasterKey)
                       .Replace("{calendarId}", calendarId)
        }

    let logicApp =
        workflow {
            name          (nameOne "la")
            resourceGroup rg.Name
            definition    (InputJson.op_Implicit logicAppJson)

            parameters [
                "$connections", workflowParameter {
                    value (InputJson.op_Implicit parametersJson)
                }
            ]
        }

    dict []
)