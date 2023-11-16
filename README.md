## Critter Stack Help Desk

This minimal service using [Wolverine](https://wolverine.netlify.app) and [Marten](https://martendb.io) as the sample application
for the talk [CQRS with Event Sourcing using the “Critter Stack”](https://www.dotnetconf.net/) at .NET Conf 2023.

To run the service, start up a PostgreSQL database and Rabbit MQ broker with:

```bash
docker compose up -d
```

from the root of this repository.


This sample was stolen from Oskar Dudycz, so see his original take on this system:

[![Twitter Follow](https://img.shields.io/twitter/follow/oskar_at_net?style=social)](https://twitter.com/oskar_at_net) [![Github Sponsors](https://img.shields.io/static/v1?label=Sponsor&message=%E2%9D%A4&logo=GitHub&link=https://github.com/sponsors/oskardudycz/)](https://github.com/sponsors/oskardudycz/) [![blog](https://img.shields.io/badge/blog-event--driven.io-brightgreen)](https://event-driven.io/?utm_source=event_sourcing_jvm) [![blog](https://img.shields.io/badge/%F0%9F%9A%80-Architecture%20Weekly-important)](https://www.architecture-weekly.com/?utm_source=event_sourcing_net)

# Pragmatic Event Sourcing With Marten

- Simplest CQRS and Event Sourcing flow using Minimal API,
- Cutting the number of layers to bare minimum,
- Using all Marten helpers like `WriteToAggregate`, `AggregateStream` to simplify the processing,
- Examples of all the typical Marten's projections,
- example of how and where to use C# Records, Nullable Reference Types, etc,
- No Aggregates! Commands are handled in the domain service as pure functions.

You can watch the webinar on YouTube where I'm explaining the details of the implementation:

<a href="https://www.youtube.com/watch?v=Lc2zV8KA16A&list=PLw-VZz_H4iiqUeEBDfGNendS0B3qIk-ps&index=11" target="_blank"><img src="https://img.youtube.com/vi/Lc2zV8KA16A/0.jpg" alt="Pragmatic Event Sourcing with Marten" width="640" height="480" border="10" /></a>

or read the articles explaining this design:

- [Slim your aggregates with Event Sourcing!](https://event-driven.io/en/slim_your_entities_with_event_sourcing/?utm_source=event_sourcing_net)
- [Event-driven projections in Marten explained](https://event-driven.io/pl/projections_in_marten_explained/?utm_source=event_sourcing_net)