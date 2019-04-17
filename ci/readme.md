*** How to login to concourse? ***
fly -t main login --team-name mobile_backend -c https://concourse-dev.gp2.axadmin.net

*** Create a new pipeline ***

fly -t main set-pipeline --pipeline [pipeline_name] --config pipeline.yml -v 