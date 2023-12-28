# Voice Recognition Module Documentation

---

## Table of Contents

- [Summary](#summary)
- [Functional Components](#functional-components)
- [Interface Definitions](#interface-definitions)
- [Class Definitions](#class-definitions)
- [Struct Definitions](#struct-definitions)

---

## Summary

The Voice Recognition Module is a component within the system designed for processing audio input. Its functionality
encompasses audio capture, voice activity detection, and speech transcription.

---

## Functional Components

- **Audio Capture**: `IMicrophoneAccess` captures raw audio data from the system�s microphone. It interfaces directly
  with audio input hardware to obtain the audio stream.

- **Voice Activity Detection (VAD)**: The module uses `SileroVAD` for detecting speech within the audio
  stream. `SileroVAD` differentiates between speech and non-speech elements in the audio, identifying segments that
  contain spoken words.

- **Buffering and Speech Detection**: In addition to VAD, the `VoiceActivityDetectionInterceptor` includes a buffering
  mechanism(`IAudioBuffer`). This buffer ensures that audio segments passed to the transcription engine are complete
  speech fragments, avoiding the processing of partial or fragmented speech.

- **Speech Transcription**: The module integrates with OpenAI's Whisper model for transcribing speech to text. This is
  done through the `IVoiceRecognition` interface, which sends speech segments to Whisper and retrieves the corresponding
  text output.

---

## Interface Definitions

### 1. ISpeechToText

**Purpose**: Interface for voice recognition engines.
**Methods**:

- `Task<string> TranscribeAudioAsync(AudioData audioData)`: Asynchronously transcribes the given audio data to text.

### 2. IVoiceActivityDetection (IVAD)

**Purpose**: Interface for asynchronous voice activity detection systems.
**Methods**:

- `Task<VoiceActivityResult> DetectAsync(AudioData audioSegment)`: Asynchronously analyzes the audio segment and returns
  details of detected voice activity.

### 3. IMicrophoneAccess

**Purpose**: Interface for accessing and controlling the microphone.
**Methods**:

- `IEnumerable<AudioData> StartRecording()`: Starts capturing audio from the microphone and returns a stream of audio
  data.
- `void StopRecording()`: Stops capturing audio from the microphone.

### 4. IAudioBuffer

**Purpose**: Buffer audio data so that individual speech segmants are not cut by the audio recordings.
**Methods**:

- `bool HasSpace(int space)`: Returns true if the buffer has enough space to store the given amount of data.
- `bool TryLoadData(AudioData audioData)`: Attempts to load the given audio data into the buffer, will fail if too full.
- `AudioData UnloadAllData()`: Unloads all data from the buffer.
- `(AudioData AudioData, int ActualSize) UnloadData(int maximumRequested)`: Unloads the given amount of data from the
  buffer, will return less if does not have all the data requested.

---

## Class Definitions

### 1. ModuleStartup

**Role**: Publishes `AudioCapturedEvent` for each chunk of audio data from `MicrophoneAccess`.

### 2. VoiceActivityDetectionInterceptor

**Role**: Manages the connection between `IVoiceActivityDetection` and an audio buffer for voice activity detection.

### 3. AudioCapturedTranscriptionEventHandler

**Role**: Handles `AudioCapturedEvent`, processes it through Whisper, and publishes `AudioTranscribedEvent`.

---

## Struct Definitions

### 1. AudioData

**Purpose**: Represents a chunk of audio data.
**Properties**:

- `short[] RawData`: The raw audio data.
- `int SamplingRate`: The sampling rate the data is in.
- `ushort ChannelCount`: The channel count the data is in.
- `ushort BitDepth`: The bit depth the data is in.
- `TimeSpan Duration`: The duration of the audio data.

### 2. AudioCapturedEvent

**Purpose**: Represents an event where audio data has been captured.
**Properties**:

- `AudioData AudioData`: The captured audio data.

### 3. AudioTranscribedEvent

**Purpose**: Represents an event where audio data has been transcribed.
**Properties**:

- `string Text`: The transcribed text from the audio data.

### 4. VoiceActivityResult

**Purpose**: Represents the result of voice activity detection.
**Properties**:

- `bool IsSpeechDetected`: Indicates whether speech was detected in the audio segment.
- `TimeSpan SpeechStart`: The start time of the detected speech in the audio segment.
- `TimeSpan SpeechEnd`: The end time of the detected speech in the audio segment.
