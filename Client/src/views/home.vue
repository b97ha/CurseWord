<template>
  <div class="content-block">
    <h2 >      Movie profanity detector    </h2>
    <h2 style="max-width:80%;padding:50px">
      Application that silence out curse words during a movie playback for kids protection, 
      done by analysing the movie closed caption (English subtitles) and look for curse words (fuck, shit, ...) 
      in a directory allowing us to detect sentences containing curse words.
      We identify the exact time at which the word was spoken and mute the player at that time.

    </h2>
  </div>
</template>

<script>

export default {
  data() {
    return {
   
    };
  },
  created() {
  
  },
  components: {
  
  }
};
</script>
