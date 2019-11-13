[![Gitter](https://badges.gitter.im/DigitalTrustProtocol/FakeNewsApp.svg)](https://gitter.im/DigitalTrustProtocol/FakeNewsApp?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

# DtpSolution

## Introduction
This is the Server side implementation of the Digital Trust Protocol.
The Protocol allows anyone, including automated software, to issue their own cryptographic identities, for the use of trust and reputation and be able to verify those of others, without the need for a trusted third party. Users issue claims to other identities, and this way build a personal web of trust network.

## Concept
The Digital Trust Protocol (DTP) is a protocol for the handling of trust in the digital space. The protocol is broadly designed to work with all aspects of trust; this includes identity, reputation, and security.
The protocol is designed to be very minimalistic and system/platform agnostic. The intention is to extend the protocol with layers above rather than direct implementations. The protocol claim message data is designed to be self-provable of authenticity, by use of private/public key algorithms and the blockchain timestamping feature. This enables the protocol to be decentralized without a need for a central authority, as servers can use a peer-to-peer communication system and verify all the data it receives individually, without having to rely on the sender.
The Protocol allows anyone, including automated software, to issue their own cryptographic identities, for the use of trust and reputation and be able to verify those of others, without the need for a trusted third party. Users issue claims to other identities, and this way build a personal web of trust network.

## How to use

Client application
[Introducing Anti Fake News](https://medium.com/@carstenkeutmann_96497/introducing-anti-fake-news-4a50cf273e6d)


### Setup

Server side solution with web server and core libraries, Written in .Net Core C# 3.0


