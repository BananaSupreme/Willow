- Now we are converting from S16 to a WAV file, but both models really want a float variation. If we do decide to do
  this we might require to resample the audio to make sure it is always 16Khz. Considering 16Khz is pretty much lower
  end of the spectrum a poor man algorithm can just skip samples.

- WE ARE SUPER SLOW! BTW, still slow, but much quicker now that the mic recording time was lowered.

- We can support officialy VOSK and Deepspeech, they are known to be on device AIs...

- SileroVAD was implemented using some code from the internet, theres much improvments that can be made there, really,
  batching, GPU and quantization might all be possible, some are certainly possible and should be looked into.

- We are resampling the audio now, but it really is a poor man algorithm

- The PvRecord library creates short[] in every frame, all the arrays for converting and manipulation audio, they can
  all clearly be reused, a pull request should be made in the original library to allow for the use of a buffer. Making
  a wrapper around the ArrayPool<T> might also be a good idea, so when we carry the data back and forth less allocations
  are made.

- Whisper is still reading it as a WAV file, we should probably convert it to a float array, and then we can use the
  same code for both models.

- Should create a way to change mics in runtime...

- Further down the line, should test if it makes more sense to not cut audio to speech points but rather just feed whole
  speech points.

- Further down the line, maybe some tests and benchmarks so we can see how good are our models performing, consider
  trade offs in deafults, allows us to offer more than a single default. Also if we ever consider different solutions to
  some of the problems we face we can actually have data to understand our problem domain. We can consider testing
  against a ground of just taking the sample and feeding it into whisper

- Further down the line, noise filtration?

- Further down the line, We can find tune the AI by using synthetic data created by GPT model to create various
  commands, then we can fit the data into audio synthesis to create a pluralistic example of sounds and data which we
  will than feed back into whisper.

- Further down the line should build a prompt builder, that knows to either look into previous input in dictation mode,
  possibly selection box context as well could be amazing, and in command mode give context into possible commands and
  possible keywords that are environment relevent, maybe give the use the ability to build their own context. It seems
  like the prompt and prefix get simply combined each allowing 244 tokens, so really we can use them all for a 488 token
  window.

I asked GPT for some datasets that might fit, might be worth looking into - 
ATIS (Air Travel Information System) Dataset: While primarily used for intent classification and slot filling tasks in the domain of airline travel, it includes spoken queries that are often short phrases or sentences.

Snips Voice Platform: This dataset was created by the Snips AI voice platform (now part of Sonos) and contains natural language utterances for various commands, like setting reminders or playing music.

Google’s Multilingual Spoken Words Corpus: An extension of the Speech Commands dataset, this includes short phrases in multiple languages, useful for command recognition.

Fluent Speech Commands (FSC): This is a dataset specifically for spoken language understanding, with audio recordings of commands for a virtual assistant. It's relatively small but highly focused on command-like utterances.

LibriSpeech: Although primarily a large-scale dataset of English read speech, derived from audiobooks, it can be a resource for extracting or studying short utterances in various contexts.

Mozilla Common Voice: An open-source dataset that's continually growing, containing voice recordings in multiple languages. It can be filtered or processed to extract shorter command-like utterances.

Custom Datasets: For very specific command structures like "move to place X," sometimes researchers create their own datasets by recording commands in controlled environments. This allows for tailoring the dataset to the exact requirements of the project.

These datasets can be used not on

- Lets vectorize the conversion between short samples and floats