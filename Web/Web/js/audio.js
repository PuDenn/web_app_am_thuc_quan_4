import { logEvent } from './api.js';

let poiId = null;
const audio = document.getElementById('audioPlayer');
const playBtn = document.getElementById('playBtn');
const progress = document.getElementById('progress');
const currentTime = document.getElementById('currentTime');
const totalTime = document.getElementById('totalTime');
const back10Btn = document.getElementById('back10Btn');

export function initAudio(id, audioScript) {
  poiId = id;
  // Dùng TTS Web Speech API vì không có file mp3
  if (!audioScript) return;
  window._audioScript = audioScript;
}

function fmt(s) {
  const m = Math.floor(s / 60), sec = Math.floor(s % 60);
  return `${m}:${sec.toString().padStart(2, '0')}`;
}

let synth = window.speechSynthesis;
let utterance = null;
let isPlaying = false;

export function setupPlayerUI(script) {
  if (!script) {
    document.querySelector('.audio-player').innerHTML =
      '<p style="color:#94a3b8;text-align:center;padding:32px">Không có nội dung thuyết minh</p>';
    return;
  }

  playBtn?.addEventListener('click', () => {
    if (isPlaying) {
      synth.cancel();
      isPlaying = false;
      playBtn.textContent = '▶';
      playBtn.classList.remove('playing-anim');
    } else {
      utterance = new SpeechSynthesisUtterance(script);
      utterance.lang = document.documentElement.lang || 'vi-VN';
      utterance.rate = 0.9;
      utterance.onend = () => {
        isPlaying = false;
        playBtn.textContent = '▶';
        playBtn.classList.remove('playing-anim');
      };
      synth.speak(utterance);
      isPlaying = true;
      playBtn.textContent = '⏸';
      playBtn.classList.add('playing-anim');
      logEvent(poiId, 'audio_play');
    }
  });

  back10Btn?.addEventListener('click', () => {
    if (isPlaying) { synth.cancel(); isPlaying = false;
      playBtn.textContent = '▶'; playBtn.classList.remove('playing-anim'); }
  });
}

// Dừng khi chuyển tab
export function stopAudio() {
  if (synth) synth.cancel();
  isPlaying = false;
  if (playBtn) { playBtn.textContent = '▶'; playBtn.classList.remove('playing-anim'); }
}
