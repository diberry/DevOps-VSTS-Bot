﻿Feature: Connect
	In order to talk to Visual Studio Team Services, we need to be able to connect to Team Services

Background: 
	Given A user 'Test User'
	And I started a conversation
	And A clean state
	And Is authorized

@Acceptance
Scenario: Connect to a account and team project.
	When I connect to the account and 'config:TeamProjectOne'
	Then I am connected to the account and 'config:TeamProjectOne'