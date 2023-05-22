<template>
  <form id="form" method="post" action="" enctype="multipart/form-data">
    <h3>Add Movie</h3>
    <div class="dx-fieldset">
      <div class="dx-field">
        <div class="dx-field-label">Movie Name:</div>
        <DxTextBox
          value=""
          v-model="movieName"
          name="MovieName"
          class="dx-field-value"
        />
      </div>
    </div>
    <div class="fileuploader-container">
      <div class="dx-field-label">Movie File:</div>
      <DxFileUploader
        :chunk-size="200000"
        ref="movie-file"
        select-button-text="Select Movie"
        upload-mode="useButtons"
        name="movieFile"
        accept="video/*"
        @change="onChange($event)"
        @value-changed="onChange"
        @uploaded="onUploaded($event)"
      />
    </div>
    <DxButton
      class="button"
      text="Add Movie"
      type="success"
      @click="onButtonClick"
      :disabled="disabled"
    />
  </form>
</template>
<script>
  import { DxFileUploader } from "devextreme-vue/file-uploader";
  import { DxTextBox } from "devextreme-vue/text-box";
  import { DxButton } from "devextreme-vue/button";
  import notify from "devextreme/ui/notify";

  export default {
    components: {
      DxFileUploader,
      DxTextBox,
      DxButton,
    },
    data() {
      return {
        movieName: "",
        disabled: false,
      };
    },
    methods: {
      onButtonClick() {
        this.disabled = true;

        this.movieFile.upload();
      },
      onChange(e) {
        var uploadUrl =
          "https://localhost:5001/api/Movies/UploadFileChunk?movieName=" +
          this.movieName;
        e.component.option("uploadUrl", uploadUrl);
      },
      onUploaded(e) {
        if (e.request.status == 200) {
          notify("Uploaded Successfuly", "success");
          setTimeout(() => {
            this.$router.push("movies");
          }, 1200);
        }
      },
    },
    computed: {
      movieFile: function () {
        //console.log(this.$refs["movie-file"])
        return this.$refs["movie-file"].instance;
      },
    },
  };
</script>
<style>
  #form {
    max-width: 600px;
    margin: auto;
  }

  .button {
    margin-top: 50px;
    margin-right: 20px;
    float: right;
  }

  .fileuploader-container {
    border: 1px solid #d3d3d3;
    margin: 20px 20px 0 20px;
  }

  #form h3 {
    margin-left: 20px;
    font-weight: normal;
    font-size: 22px;
  }
  .dx-fileuploader-upload-button {
    display: none;
  }
</style>
