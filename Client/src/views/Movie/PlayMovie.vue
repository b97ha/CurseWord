
<template>
  <div class="content-block">

    <vue-core-video-player  :src="src"></vue-core-video-player>

  </div>
</template>

<script>

export default {
  data() {
    return {
     
    };
  },
  computed:{
      src(){
          return this.$store.getters.getBaseUrl + "/MutedVideo/" + this.$route.params.fileName
      }
  },
  created() {
  
  },

};
</script>
