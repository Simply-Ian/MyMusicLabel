The project is a university practical task puprosed to train domain modelling and C# language skills. The application is a simplified tool designed for employees of small independent music labels. Read more about features the app provides and domain description at `MyMusicLabel.Domain/Предметная область одним файлом.md`. **IMPORTANT NOTE**: the application does not actually integrate with any streaming platform like Spotify etc., it only pretends to do so and generates believable listening numbers instead of retrieving them via API. Neither it stores its data in any kind of database (even in-memory one). To facilitate simulation, the application allows user specify the date instead of the current one at which each action takes place; the rest of the layers are not aware of it.

The application is a desktop single-user console app. No authorization is required, no collaborative work, etc.

Architecturally the solution is divided into three projects according to Clean Architecture recommendations:
* `MyMusicLabel.csproj`: this project represents application layer
* `MyMusicLabel.Domain.csproj`: this is where domain is described
* `MyMusicLabel.Infrastructure.csproj`: it contains realizations of repositories and fake service that imitates integration with a streaming platform API.

The domain was described and implemented manually according to DDD techniques. **You are not allowed to modify any files at domain level. If you need to adjust interfaces of domain components or have noticed an error, request for appropriate changes or notify me explicitly via chat**. There are only 2 aggregates currently: `Track` and `Artist`. `Release` entity is a part of `Artist` aggregate. The supported invariants and business rules are described at `MyMusicLabel.Domain/Предметная область одним файлом.md`. To perform required tasks, application layer may address domain services. Currently there are two of them: 
* `TrackProductionDomainService` performs actions related to composing/recording tracks and establishing royalties to artists. It provides a Fluent API to the client code: e. g. to add music to a track for which the composer will receive interest for every listening, call `service.ProduceTrack(trackid).AddMusic(composerId, durationSeconds).ForCharge(interest, todayDate)`
* `StreamingPlatformDomainService` performs actions related to release publication at streaming platforms and counting up income/royalties. It operates with various instances of `IStreamingPlatformAppService` which act as adapters to different streaming platforms` API.

The application layer code (primarily application services) may use public methods of entities & aggregates as well if needed.

Side packages are avoided in the project. Currently only Microsoft.Extensions.DependencyInjection, Microsoft.Extensions.DependencyInjection.Abstractions and MoreLinq are adopted. FluentValidation is also installed and must be used for user input validation at the apllication layer. **Do not install any packages. If you think some is needed, request me to install it explicitly but better try to find a workaround**.

No CI/CD pipeline is built around

The following code conventions are applied to the project:
* Exceptions are the preferred way of errors handling. "Result" pattern is avoided. Domain and infrastructure exceptions are handled at the application layer. **You may not introduce any non-standard exception classes.**
* Comments are written in Russian, **but you are not allowed to place any comments**
* Agents **are not allowed to commit any files** (I will revert such commits and preserve changes manually, anyway).
* Only synchronous method invocations are allowed and no asynchronous methods should be defined since the application does not communicate with side APIs via network nor with database, etc.
* All user commands (MyMusicLabel.Commands.ICommand implementations) are annotated with CommandName attribute. Its only parameter is
  * Command name if the command belongs to no group
  * Group name + command name if the command represents an action over a certain instance of an entity
  * Group name + "list" if the command is purposed to print list of all the available entities of a certain type
  Group names usually correspond to entities they are performed over