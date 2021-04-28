# AutoFixtureMoqDemo
Demo for showing how Autofixture and Moq can improve unit tests.

A console app mimicking a store application with a CQRS architecture.

Has 4 versions of unit tests for the AddToCartRequestHandler:

* V0 doesn't use Moq or Autofixture.
* V1 uses Moq.
* V2 uses Autofixture.
* V3 uses Autofixture.AutoMoq.

Comments are present in tests to hopefully motivate/explain why Moq and Autofixture can be helpful.
