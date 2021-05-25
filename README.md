# Container Serverless
![dotnet test](https://github.com/jborup/container-serverless/actions/workflows/dotnet.yml/badge.svg)
[![Container build](https://quay.io/repository/jborup/helloworld-csharp/status "Docker Repository on Quay")](https://quay.io/repository/jborup/helloworld-csharp)

Serverless is great to avoid the need of thinking about the underlying infrastructure.

Cloud specific serverless such as [AWS Lambda](https://aws.amazon.com/lambda/), [GCP App Engine](https://cloud.google.com/appengine), [Azure App Service](https://docs.microsoft.com/en-us/azure/app-service/) and [IBM Functions](https://cloud.ibm.com/functions/) are super great. All of them are highly optimized and works perfect in the respective cloud.

However it locks into this specific Cloud serverless aproach, and how do you actually develop, debug, test it locally? In many cases it becomes harder to develop locally, end to end.

This is where containers becomes a good compromise. Containers ensures you can run it all "locally", and can also in clouds. Or as some call it "build once, deploy/run anywhere".

Lets take a journey into running a simple application that is first build locally, then tested locally (via containers, hence it could be tested in cloud too), deployed to "the clouds".

## A developer friendly way...
Container based serverless might be the approach. As long as you can build a container image, you can run it on any of the cloud providers, in private cloud and also locally. All of them in the same way with the same "contract" to the underlyaing OS.

All of the cloud providers have their own version of this, and there is also a standard evolving for this named [Knative](https://knative.dev/), at the time of this writting it is still (only) version 0.22, as Google is driving this there is a high likelihood it will become standard going forward, though it is not under the CNCF.io umbrella.

Meanwhile the cloud providers and their own implementation:
* [Amazon Elastic Container Service](https://aws.amazon.com/ecs) 
* [Azure Container Instances](https://portal.azure.com/#create/Microsoft.ContainerInstances)
* [Google Cloud Run](https://cloud.google.com/run)
* [IBM Code Engine](https://cloud.ibm.com/codeengine)

And of course Docker Desktop locally:
* [Docker Desktop](https://www.docker.com/products/docker-desktop)

A lot of effort is going into ensuring no or less need to understand Dockerfiles, deployment YAML files etc. However some knowledge still is *VERY* valuable.

## The "hello world" TDD example
Lets do a simple application for Dave the developer.
Lets use these user stories:

* As Dave I want to use C# as I know the language, and do it in Containers
* As Dave I always wants to write test cases before the code (Xunit)
* As Dave I want to see `hello world` as response
* As Dave I want to be able to greet a different TARGET (via TARGET ENVIRONMENT VARIABLE)

### TDD: I want to use C# in container
Heavy inspiration from [GCP Run C# example](https://cloud.google.com/run/docs/quickstarts/build-and-deploy/c-sharp).

Prereq: Have [.NET Core SDK 3.1](https://dotnet.microsoft.com/download) installed (So that `dotnet` works).
(TL;DR: wget https://dot.net/v1/dotnet-install.sh && ./dotnet-install.sh -c 3.1 && ~/.dotnet/dotnet --version)

### TDD: Hello world

Given my end-point (no arguments), I just want to see `hello world` as reponse.

```bash
# Leave out the alias command if you have dotnet installed "global" 
alias dotnet=~/.dotnet/dotnet
dotnet new web -o helloworld-csharp

cd helloworld-csharp
dotnet restore
#dotnet tool update -g dotnet-format
#dotnet format --check
#dotnet test
#dotnet publish -c Release -o out
#dotnet bin/Release/netcoreapp3.1/helloworld-csharp.dll 

docker build -t helloworld-csharp .
docker run -it -p 8080:8080 -e TARGET=local helloworld-csharp
```

## Push image to container registry (public)
Using `quay.io` to avoid any rate limit issues, if this is ignored could use `docker.io` as well.
```bash
docker login quay.io -u jborup -p XXXXXXXXXXX
docker tag helloworld-csharp quay.io/jborup/helloworld-csharp:1
docker push quay.io/jborup/helloworld-csharp:1
```


## Hello Azure
We will be using command line for this, which is assumed installed. Also we use the resource group named `test-rg` which is assumed to exist up front. (If not can be created like this: `az group create --name test-rg --location westeurope`)

```bash
az login
az container create --resource-group test-rg --name helloworld-csharp --image quay.io/jborup/helloworld-csharp:1 --dns-name-label helloworld-csharp --ports 80 --environment-variables TARGET=azure PORT=80
az container list -o table
az container show --resource-group test-rg --name helloworld-csharp -o table
# Lets find the hostname
az container show --resource-group test-rg --name helloworld-csharp|jq .ipAddress.fqdn
FQDN=`az container show --resource-group test-rg --name helloworld-csharp|jq .ipAddress.fqdn | tr -d \" `
curl $FQDN
az container delete --resource-group test-rg --name helloworld-csharp

```

[Azure Command Line documentation](https://docs.microsoft.com/en-us/cli/azure/container?view=azure-cli-latest)

## Hello AWS Fargate
We will be using command line for this, which is assumed installed.
We will also be using he AWS Fargate model.


```bash
# PENDING

```

## Hello GCP
We will be using command line for this, which is assumed installed.
(TL;DR: https://cloud.google.com/sdk/docs/install#deb ....... gcloud init && gcloud services enable containerregistry.googleapis.com && gcloud auth configure-docker
)


```bash
docker tag quay.io/jborup/helloworld-csharp:1 gcr.io/helloworld-serverless/hw:1
docker push gcr.io/helloworld-serverless/hw:1

gcloud run deploy hw \
--image=gcr.io/helloworld-serverless/hw:1 \
--allow-unauthenticated \
--max-instances=1 \
--platform=managed \
--region=us-central1 \
--set-env-vars=TARGET=GCP \
--project=helloworld-serverless

```

## Hello IBM
We will be using command line for this, which is assumed installed. (And the Code Engine plugin `ibmcloud plugin install code-engine` - see [IBM Cloud Code Engine CLI](https://cloud.ibm.com/docs/codeengine?topic=codeengine-cli))

```bash
ibmcloud login
ibmcloud target -g Default
ibmcloud ce project create --name test
ibmcloud ce application create --name hw --image quay.io/jborup/helloworld-csharp:1
ibmcloud ce application get -n hw -output url

```

# Comparison

| Cloud                            | AWS | Azure   | GCP  | IBM   | Private |
| -------------------------------- | --- | ------- | ---- | ----- | ------- |
| Any (public) Container registry  | Yes |  Yes    | No   | Yes   | Yes     |
| Option for own FQDN              | ?   |  No     |Pre-GA|  No   | Yes     |
| Minimum container size - cores   | 0.25|  1      | 1    | 0.125 |  -      |
| Minimum container size - mem GB  | 0.5 |  0.5    | 0.25 | 0.25  |  -      |
| Minimum hourly list price (DKK)  | 0.06|  0.33   |  ?   | 0.01  |  -      |
| Minimum monthly list price (DKK) | ~44 |  242.12 | ~50  | 2.17  |  -      |
| Can scale to zero instances?     |  ?  |   No    | Yes  | Yes   |  -      |


All disclaimers apply on prices, as this is not something I am in control off. I have tried to find the prices (beyond the freemium base) for consuming 1 hour and 1 month (31 days of 24 hours).

More info on [FQDN Pre-GA on Google](https://cloud.google.com/run/docs/mapping-custom-domains)

# TODO
* Add using private container registry and setting authentication
* Run own domain

# Links

* [Azure Cost Calculator](https://azure.microsoft.com/en-us/pricing/calculator/)
* [Google Cloud Pricing Calculator](https://cloud.google.com/products/calculator)
* [AWS Pricing Calculator](https://calculator.aws/#/)
* [AWS Fargate Pricing](https://aws.amazon.com/fargate/pricing/)
* [IBM Cloud Cost](https://cloud.ibm.com/)
