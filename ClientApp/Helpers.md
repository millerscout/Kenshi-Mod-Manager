# Helpers

### How to show confirm menu

``` javascript
this.$store
				.dispatch("storeDialog/confirm", {
					title: `The Title `,
					body: `Are you sure?`
				})
				.then(confirmation => {
					if (confirmation) {
					//some action 
					} else {
						return;
					}
        });
```

### Sandbox mode
``` javascript  
    /// after "const store",  add the following line.
    store.dispatch("storeConfig/setSandbox", true); 
```