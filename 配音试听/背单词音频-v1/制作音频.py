from pathlib import Path
import wave
import numpy as np

ROOT = Path(__file__).resolve().parent
SR = 44100
RNG = np.random.default_rng(603)

def write(name, audio):
    audio = np.asarray(audio)
    if audio.ndim == 1:
        audio = np.column_stack((audio, audio))
    peak = np.max(np.abs(audio))
    if peak > .88:
        audio *= .88 / peak
    with wave.open(str(ROOT / name), 'wb') as out:
        out.setnchannels(2)
        out.setsampwidth(2)
        out.setframerate(SR)
        out.writeframes((np.clip(audio, -1, 1) * 32767).astype('<i2').tobytes())

def hz(midi):
    return 440 * 2 ** ((midi - 69) / 12)

def note(midi, duration, flavor='lead'):
    t = np.arange(int(duration * SR)) / SR
    f = hz(midi)
    if flavor == 'lead':
        # Rounded triangle-like tone, retaining the retro character without harsh edges.
        sound = sum(((-1) ** ((h - 1) // 2)) * np.sin(2*np.pi*f*h*t) / h**2 for h in (1, 3, 5, 7))
        env = np.minimum(t / .012, 1) * np.minimum((duration-t) / .075, 1) * np.exp(-t*.65)
    elif flavor == 'pluck':
        sound = np.sin(2*np.pi*f*t) + .3*np.sin(2*np.pi*f*2*t) + .13*np.sin(2*np.pi*f*3*t)
        env = np.minimum(t/.006, 1)*np.exp(-t*10)*np.minimum((duration-t)/.04, 1)
    elif flavor == 'bass':
        sound = np.sin(2*np.pi*f*t) + .2*np.sin(2*np.pi*f*2*t)
        env = np.minimum(t/.016, 1)*np.minimum((duration-t)/.06, 1)*np.exp(-t*1.7)
    else:
        sound = np.sin(2*np.pi*f*t) + .15*np.sin(2*np.pi*f*2*t)
        env = np.minimum(t/.15, 1)*np.minimum((duration-t)/.18, 1)
    return sound * np.maximum(env, 0)

BEAT = 60 / 96
DURATION = 16 * 4 * BEAT  # Exactly 40 seconds, suitable for sample-accurate looping.
music = np.zeros((int(DURATION*SR), 2))

def add_loop(sound, start, gain, pan=0):
    indices = (int(start*SR) + np.arange(len(sound))) % len(music)
    np.add.at(music[:,0], indices, sound*gain*np.sqrt((1-pan)/2))
    np.add.at(music[:,1], indices, sound*gain*np.sqrt((1+pan)/2))

chords = [(50,57,60,65),(48,55,60,64),(43,55,59,62),(45,57,60,64)]
melodies = [
    [(0,74,1), (1,77,.5), (1.5,76,.5), (2,74,1.5)],
    [(0,72,1), (1.5,76,.5), (2,79,1), (3,76,.75)],
    [(0,71,1), (1,74,1), (2.5,76,.5), (3,74,.75)],
    [(0,72,1.5), (2,69,1.5)],
    [(0,74,.75), (1,77,.75), (2,81,1), (3,79,.75)],
    [(0,79,1.5), (2,76,.75), (3,72,.75)],
    [(0,74,1), (1.5,71,.5), (2,69,1), (3,71,.75)],
    [(0,72,1.5), (2,74,1.5)],
]
for bar in range(16):
    start = bar*4*BEAT
    chord = chords[bar%4]
    for voice, midi in enumerate(chord[1:]):
        add_loop(note(midi, 4*BEAT, 'pad'), start, .015, (-.5,0,.5)[voice])
    for beat in (0, 2):
        add_loop(note(chord[0] if beat == 0 else chord[0]+7, .8*BEAT, 'bass'), start+beat*BEAT, .105)
    for step in range(8):
        midi = chord[1+step%3]+12
        add_loop(note(midi, .25, 'pluck'), start+step*.5*BEAT, .023, -.55 if step%2==0 else .55)
    for offset, midi, length in melodies[bar%8]:
        if bar>=8 and bar%4==0:
            midi += 12
        add_loop(note(midi, length*BEAT, 'lead'), start+offset*BEAT, .066, .08)
    for beat in range(4):
        t = np.arange(int(.15*SR))/SR
        if beat%2 == 0:
            phase = 2*np.pi*(45*t+45*.025*(1-np.exp(-t/.025)))
            drum = np.sin(phase)*np.exp(-t*30)
            add_loop(drum, start+beat*BEAT, .075)
        else:
            noise = RNG.normal(0,1,len(t))
            noise = np.convolve(noise,np.ones(5)/5,mode='same')
            add_loop(noise*np.exp(-t*48), start+beat*BEAT, .037, .18)
    for beat in np.arange(0,4,.5):
        t = np.arange(int(.035*SR))/SR
        noise = RNG.normal(0,1,len(t))
        high = noise - np.convolve(noise,np.ones(9)/9,mode='same')
        add_loop(high*np.exp(-t*150),start+beat*BEAT,.008,-.22)

# A short stereo echo is mixed circularly, retaining the loop's reverb tail.
music += np.roll(music, int(BEAT*.75*SR), axis=0)*.10
music += np.roll(music[:,::-1], int(BEAT*1.5*SR), axis=0)*.045
write('01-林间远征-背景音乐-40秒循环.wav', music)

t = np.arange(int(.28*SR))/SR
noise = RNG.normal(0,1,len(t))
noise = np.convolve(noise,np.ones(7)/7,mode='same')
phase = 2*np.pi*(1600*t-2100*t*t)
slash = .23*noise*np.sin(np.pi*np.minimum(t/.18,1))**2*np.exp(-t*9)
slash += .11*np.sin(phase)*np.minimum(t/.003,1)*np.exp(-t*22)
slash += .20*np.sin(2*np.pi*180*t)*np.exp(-np.maximum(t-.045,0)*45)*(t>=.045)
slash *= np.minimum((.28-t)/.03,1)
write('02-攻击-像素挥击.wav',slash)

t = np.arange(int(.42*SR))/SR
phase = 2*np.pi*(50*t+100*.04*(1-np.exp(-t/.04)))
noise = np.convolve(RNG.normal(0,1,len(t)), np.ones(9)/9,mode='same')
hit = .33*np.sin(phase)*np.exp(-t*14)+.23*noise*np.exp(-t*38)
hit += .08*np.sin(2*np.pi*330*t)*np.exp(-t*28)
hit *= np.minimum(t/.002,1)*np.minimum((.42-t)/.04,1)
write('03-受击-像素撞击.wav', hit)

demo = music[:int(12*SR)].copy()
for sec, effect in [(2.5,slash),(5.5,hit),(8,slash),(9,slash)]:
    offset=int(sec*SR)
    demo[offset:offset+len(effect)] += effect[:,None]*.65
demo[:int(.12*SR)] *= np.linspace(0,1,int(.12*SR))[:,None]
demo[-int(.6*SR):] *= np.linspace(1,0,int(.6*SR))[:,None]
write('04-战斗混合试听-12秒.wav', demo)
print('Created 4 original stereo PCM WAV previews at 44100 Hz.')
