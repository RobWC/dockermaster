using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Docker.DotNet;
using Docker.DotNet.Models;
using System.Threading.Tasks;

namespace Dockermaster
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            DockerClient client = new DockerClientConfiguration(new Uri("unix://var/run/docker.sock")).CreateClient();

            try
            {
                
           
                ListContainers(client).Wait();
                StartContainer(client).Wait();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.WriteLine("Goodbye World!");

        }

        static async Task StartContainer(DockerClient client)
        {
            var newUUID = System.Guid.NewGuid();
            Console.WriteLine("Created ID {0}", newUUID);
            Char[] buffer;
            
            await client.Images.CreateImageAsync(new ImagesCreateParameters { FromImage = "nginx", Tag = "latest" }, null, IgnoreProgress.Forever);
             
            await client.Containers.CreateContainerAsync(
                new CreateContainerParameters
                { 
                    Image = "nginx", 
                    Name = $"nginx-{newUUID}-dm", 
                    Tty = true,
                    HostConfig = new HostConfig
                    {
                        PortBindings = new Dictionary<string, IList<PortBinding>>
                        {
                            { 
                                "80/tcp", 
                                new List<PortBinding> { 
                                    new PortBinding
                                    {
                                        HostPort = "8888"
                                    } 
                                }
                            }
                        }
                    }
                });
            // Starting the container ...
            await client.Containers.StartContainerAsync($"nginx-{newUUID}-dm", new ContainerStartParameters { });
            
            Console.WriteLine("Made thing?");
        }

        static async Task ListContainers(DockerClient client)
        {
            
            Console.WriteLine("Listing containers");
            // list containers
            IList<ContainerListResponse> containers = await client.Containers.ListContainersAsync(
                new ContainersListParameters(){
                    Limit = 10,
                });

            foreach (var container in containers)
            {
                foreach (var name in container.Names)
                {
                    Console.WriteLine("Container Name: {0}", name);
                    if (Regex.IsMatch(name, @"^/cock", RegexOptions.None))
                    {
                        Console.WriteLine("We found the container!");
                    }
                }
            }
            Console.WriteLine("Listing containers complete");
        }
        
        private class IgnoreProgress : IProgress<JSONMessage>
        {
            public static readonly IProgress<JSONMessage> Forever = new IgnoreProgress();

            public void Report(JSONMessage value)
            {
                Console.WriteLine(value.Status);
            }
        }
    }
}