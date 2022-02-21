module Program

open Pulumi.FSharp.AzureNative.Components.FunctionAppPackage
open Pulumi.FSharp.NamingConventions.Azure.Region
open Pulumi.FSharp.AzureNative.Resources
open Pulumi.FSharp.Config
open Pulumi.FSharp
open Pulumi

Deployment.run (fun () ->
    let workload =
        config.["workloadOrApplication"]

    let rg =
        resourceGroup {
            name $"rg-{workload}-{Deployment.Instance.StackName}-{shortName}-001"
        }

    let functionAppInfrastructure =
        functionAppPackage {
            workloadName              workload
            resourceGroupName         rg.Name
            projectPackagePublishPath config.["projectPublishPath"]
        }

    dict []
)