# Tharga Cache

- ForeverCache (IForeverCache)
- ForeverDistributedCache (IForeverDistributedCache)
- RollingCache (IRollingCache)
- RollingDistributedCache (IRollingDistributedCache)
- TimeCache (TimeCache)
- TimeDistributedCache (ITimeDistributedCache)
- BackgroundCache (IBackgroundCache)
- BackgroundDistributedCache (IBackgroundDistributedCache)

## TODO
- Add distribute provider
- Test for distributed "LOCAL"
- Test for distributed "DISABLED"
- Test for distributed (on different instances)
- Test local (different instances) ==> Should be isolated.

- Test for expired values
- Tests for background tasks that should load values in the background, trigger an event and then be updated.

- Rolling distributed cache does not automatically unload values, since it does not know when to do it.
- Tests for rolling cache, should be keept in memory as long as it is used.

- Make sure dual calls does not make more database calls
- Make it possible to opt out from refresh-calls in the background-cache. (Just load what is stored)
- Make it possible to throttle caches (If I have 1000-caches and all tries to load at the same time, especially for the background cahce version)
