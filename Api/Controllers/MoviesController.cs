using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.TranscribeService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MovieProfanityDetector.Data.UnitOfWork;
using MovieProfanityDetector.Helpers;
using MovieProfanityDetector.Models;
using MovieProfanityDetector.Models.Base;
using MovieProfanityDetector.Models.Entities;
using Newtonsoft.Json;

namespace MovieProfanityDetector.Controllers
{
    public class MoviesController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly BaseResponse _response;
        private readonly IUploader _uploader;
        private readonly IFFmpeg _fmpeg;
        private readonly AwsSettings _options;
        private readonly IAwsSpeech _awsSpeech;

        public MoviesController(
            IOptions<AwsSettings> options,
            IUnitOfWork unitOfWork,
            BaseResponse response, 
            IUploader uploader,
            IFFmpeg fmpeg,
            IAwsSpeech awsSpeech
            )
        {
            _unitOfWork = unitOfWork;
            _response = response;
            _uploader = uploader;
            _fmpeg = fmpeg;
            _awsSpeech = awsSpeech;
            _options = options.Value;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            _response.Result = await _unitOfWork.Movies.SelectAll();
            return Ok(_response);
        }


        [HttpPost]
        public async Task<IActionResult> UploadFileChunk([FromForm] IFormFile movieFile,
            [FromForm] string chunkMetadata, [FromQuery] string movieName)
        {
            try
            {
               
                if (movieFile != null)
                {
                    var metaDataObject = JsonConvert.DeserializeObject<ChunkMetaData>(chunkMetadata);

                    if (metaDataObject.Index < (metaDataObject.TotalCount - 1))
                    {
                        var file =  await _uploader.UploadChunk(movieFile, metaDataObject, Guid.Empty);
                        if (!file.IsUploaded)
                        {
                            _response.Message = file.Message;
                            return Ok(_response);
                        }
                    }
                    else
                    {
                        var movie = new Movie
                        {
                            MovieName = movieName,
                        };

                        var file = await _uploader.UploadChunk(movieFile, metaDataObject, movie.Guid);

                        if (file.IsUploaded)
                        {
                            await _fmpeg.ExtractAudio(file.FilePath, file.FileName);

                            movie.FileName = file.FileName;

                            await _unitOfWork.Movies.CreateAsync(movie);
                            var save = await _unitOfWork.CompleteAsync();
                        }
                    }

                    return Ok(_response);
                }
            }
            catch (Exception e)
            {
                _response.Message = e.Message;
            }

            _response.Code = StatusCodes.Status400BadRequest;
            return Ok(_response);
        }


        [HttpGet]
        public async Task<IActionResult> GetTranscribe(Guid guid)
        {
            try
            {
                var movie = await _unitOfWork.Movies.SelectById(guid);
                if (movie.IsTranscription)
                {
                       var transPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", Path.Combine("Transcriptions", guid +".json"));
                       if (System.IO.File.Exists(transPath))
                       {
                           var text = await System.IO.File.ReadAllTextAsync(transPath);
                           _response.Result = JsonConvert.DeserializeObject<TranscriptionJobResult>(text);
                       }
                }
                else
                { 
                    var trans = await _awsSpeech.TranscribeInputFile(guid);
                    if (trans.status == TranscriptionJobStatus.COMPLETED)
                    {
                        movie.IsTranscription = true;
                        movie.TransJobName = trans.jobName;

                        _unitOfWork.Movies.Update(movie);
                        await _unitOfWork.CompleteAsync();
                        _response.Result = trans;

                    }
                }
            }
            catch (Exception e)
            {
                _response.Message = e.Message;
                _response.Code = StatusCodes.Status400BadRequest;
            }

            return Ok(_response);
        }

        
        [HttpGet]
        public async Task<IActionResult> DetectProfanity(Guid guid)
        {
            try
            {
                var movie = await _unitOfWork.Movies.SelectById(guid);

                await _fmpeg.MuteVideo(guid);
                await _fmpeg.ConcatVideo(guid, movie.FileName);
            }
            catch (Exception e)
            {
                _response.Message = e.Message;
                _response.Code = StatusCodes.Status400BadRequest;
            }
                
            return Ok(_response);

        }
        
    }
}