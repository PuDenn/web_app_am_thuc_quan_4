import { useEffect } from 'react'
import { X, Volume2 } from 'lucide-react'
import { useAppStore } from '../../stores/useAppStore'

/**
 * AudioPlayer: Hiển thị thanh mini player khi đang phát audio.
 * Dùng Web Speech API (vi-VN) để simulate TTS.
 * Trong production: thay bằng <audio src={poi.audioFile}> hoặc streaming TTS API.
 */
export default function AudioPlayer() {
  const { isAudioPlaying, currentAudioPoi, stopAudio } = useAppStore()

  useEffect(() => {
    if (isAudioPlaying && currentAudioPoi) {
      window.speechSynthesis.cancel()

      const utterance = new SpeechSynthesisUtterance(
        `Chào mừng bạn đến ${currentAudioPoi.name}. ${currentAudioPoi.description} Giá từ ${currentAudioPoi.priceRange}.`
      )
      utterance.lang = 'vi-VN'
      utterance.rate = 0.9
      utterance.onend = () => stopAudio()
      window.speechSynthesis.speak(utterance)
    } else {
      window.speechSynthesis.cancel()
    }

    return () => {
      window.speechSynthesis.cancel()
    }
  }, [isAudioPlaying, currentAudioPoi, stopAudio])

  if (!isAudioPlaying || !currentAudioPoi) return null

  return (
    <div className="absolute bottom-4 left-4 right-4 z-20 bg-gray-900 text-white rounded-2xl p-4 flex items-center gap-3 shadow-xl">
      {/* Waveform animation */}
      <div className="flex items-center gap-0.5">
        {[1, 2, 3, 4].map((i) => (
          <div
            key={i}
            className="w-1 bg-orange-400 rounded-full animate-bounce"
            style={{ height: `${8 + i * 4}px`, animationDelay: `${i * 0.1}s` }}
          />
        ))}
      </div>

      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-1.5 text-xs text-orange-400 font-semibold mb-0.5">
          <Volume2 size={12} />
          Đang phát
        </div>
        <p className="text-sm font-bold truncate">{currentAudioPoi.name}</p>
      </div>

      <button
        onClick={stopAudio}
        className="p-1.5 rounded-full hover:bg-gray-700 flex-shrink-0"
      >
        <X size={18} />
      </button>
    </div>
  )
}
