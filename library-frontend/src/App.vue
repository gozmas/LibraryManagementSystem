<template>
  <router-view />
</template>

<script>
export default {
  name: 'App'
}
</script>

<style>


* {
  box-sizing: border-box;
  margin: 0;
  padding: 0;
}
html, body, #app {
  height: 100%;
  min-height: 100vh;
}
body {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
}
a {
  text-decoration: none;
  color: inherit;
}

</style>