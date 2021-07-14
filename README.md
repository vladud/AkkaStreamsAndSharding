# AkkaStreamsAndSharding

This project tries to combine akka sharding and akka streams in order to achieve a scalable cluster that can run some algorithm built with akka streams.

The implementation seems to show that there are some performance issues inside akka when combining both sharding and streams.

Scenarios
* When running with a smaller number of graphs (which means smaller number of actors inside the cluster as well) - then the cluster is formed without any issues (second node is able to join)
* When running with a larger number of graphs and big value set for auto-down-unreachable-after - then the cluster is slow but it manages to form eventually
* Otherwise the second node is marked as unreachable

** The cluster is also formed ok if no messages are being sent until both nodes are joined. If the input starts after both nodes are up then the sharding works as expected

There are powershell scripts included to run all these scenarios.
