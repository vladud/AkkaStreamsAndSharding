# AkkaStreamsAndSharding

This project tries to combine akka sharding and akka streams in order to achieve a scalable cluster that can run some algorithm built with akka streams.

The implementation seems to show that there are some performance issues inside akka when combining both sharding and streams.

Scenarios
* When running with a smaller number of graphs (which means smaller number of actors inside the cluster as well) - then the cluster is formed without any issues (second node is able to join) but it takes a bit of time
* When running with a larger number of graphs and big value set for auto-down-unreachable-after (or set it off) - then the cluster is slow but it manages to form eventually (though initially we do get a unreacheble log)
* Otherwise the second node is marked as unreachable

** The cluster is also formed ok if no messages are being sent until both nodes are joined. If the input starts after both nodes are up then the sharding works as expected

There are powershell scripts included to run all these scenarios.


*** This project uses separate actor systems for the graph and the sharding. While testing there was huge performance issues in building the graph (just running materialize on it) when using the ActorMaterializer from inside the sharding actor system (see StreamSourceWithInternalGraphBuildingActor for example)
