# Region Deployer

Deploy an azure function to every available region.

perform an http request across all regions.

## setup

- Install .net desktop development pack

- Install F# desktop development component

```
nuget restore
```

- unload each project and reload them into solution

## Testing 

- database initial state

	running the test suite in this solution refreshes the database state, specifically the databaseseeding.fs file. 

	if it fails, the state will be corrupted.

- run in isolation

	tests should be run one by one

	this is to ensure each test has a consistent initial state.

- speed

	these are mostly cloud integration tests. they take a long time to pass.

	sometimes the name of a cloud resource was not random enough and it can return a "conflict result." rerun to get this working.

- setup
	the tests wont pass if groups already exist. 
	
	plan to redeploy after you are done with testing.
