using System.Linq;
using Nuke.Common;
using Nuke.Common.Tools.Docker;
using static Nuke.Common.Tools.Docker.DockerTasks;

partial class Build
{
    const string RabbitMqImageName = "docker.io/library/rabbitmq:latest";
    const string RabbitMqContainerName = "backendfx-rabbitmq";

    Target StartDependencies
        => _ => _
               .Executes(() =>
                         {
                             var existingContainers = DockerPs(s => s
                                                                   .SetFilter($"name={RabbitMqContainerName}")
                                                                   .EnableAll()
                                                                   .EnableQuiet());
                             if (existingContainers.Any())
                             {
                                 DockerRm(s => s
                                              .AddContainers(existingContainers.Select(c => c.Text))
                                              .EnableForce());
                             }

                             DockerPull(x => x.SetName(RabbitMqImageName).EnableQuiet());
                             
                             DockerRun(x => x
                                           .SetImage(RabbitMqImageName)
                                           .SetName(RabbitMqContainerName)
                                           .SetEnv("RABBITMQ_DEFAULT_USER=test", "RABBITMQ_DEFAULT_PASS=password")
                                           .SetPublish("5672:5672")
                                           .EnableDetach());
                         });

    Target StopDependencies
        => _ => _
               .AssuredAfterFailure()
               .Executes(() => DockerStop(x => x.SetContainers(RabbitMqContainerName)));
}
