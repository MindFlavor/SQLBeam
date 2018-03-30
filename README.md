# SQLBeam

ToDo

## Known issues

### HttpListenerException: Access is denied

This happens because the REST interface uses port 9000 and that port access has not been granted to the executing user. In order to correct it issue this command from an administrative prompt:

```
netsh http add urlacl url=http://+:9000/api user=DOMAIN\user
```
 
Where `DOMAIN\user` is the account starting the program (it will be you if running from VS)


