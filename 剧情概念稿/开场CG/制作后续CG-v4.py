from pathlib import Path
import subprocess
import wave
import json
import numpy as np

base=Path(__file__).resolve().parent
root=base.parents[1]
ffmpeg=str(root/'tools/ffmpeg/ffmpeg.exe')
sr=44100
font='C\\:/Windows/Fonts/msyh.ttc'
shots=[
 dict(folder='第三段-雨夜降落',duration=8,scene='第三段-雨夜降落',sound='landing',lines=[(.8,4,'这里不是我的宿舍……',"This isn't my dorm room..."),(4.3,7.8,'我到底到了什么地方？','Where on earth am I?')]),
 dict(folder='第四段-系统觉醒',duration=19,scene='第三段-雨夜降落',sound='system',lines=[(.4,3.5,'万象系统已觉醒。','The Myriad System has awakened.'),(3.8,7.3,'完成词汇挑战，获得金币。','Complete vocabulary challenges to earn coins.'),(7.6,11.3,'金币可购买装备、道具与技能。','Spend coins on equipment, items and skills.'),(11.6,15.3,'部署支援能力，帮助异世界中的伙伴。','Deploy support abilities to help people in other worlds.'),(15.6,18.8,'我穿越了……还得背单词？',"Another world... and I still have to study words?")]),
 dict(folder='第五段-红光扫描',duration=8,scene='第五段-红光扫描',sound='scan',lines=[(.5,3.6,'警告：本地身份记录缺失。','Warning: no local identity record found.'),(3.9,7.8,'等等……它在扫描我！',"Wait... it's scanning me!")]),
 dict(folder='第六段-星遥登场',duration=8,scene='第六段-星遥登场',sound='run',lines=[(.6,3.6,'身后忽然响起急促的脚步声。','Rapid footsteps suddenly sound behind me.'),(3.9,7.8,'别站在那里！跟我走！',"Don't stand there! Come with me!")]),
 dict(folder='第七段-暗巷暂避',duration=10,scene='第七段-暗巷暂避',sound='shelter',lines=[(.4,3.6,'你没有身份记录，对吧？',"You don't have an identity record, do you?"),(3.9,7,'首次支援目标确认：陈星遥。','First support target confirmed: Chen Xingyao.'),(7.4,9.8,'第一卷 · 霓虹失名者','Volume I: The Nameless of Neon')]),
]

def make_sound(shot,folder):
    duration=shot['duration'];kind=shot['sound'];rng=np.random.default_rng(460+shots.index(shot));t=np.arange(int(duration*sr))/sr
    def filtered(data,count):return np.convolve(data,np.ones(count)/count,mode='same')
    # Rain has a broad outdoor hiss, gusts, discrete droplets and distant traffic.
    # This replaces the bedroom fan's narrow, steady noise texture.
    noise=rng.normal(0,1,(len(t),2));mix=np.zeros_like(noise)
    for channel in range(2):
        mix[:,channel]=filtered(noise[:,channel],3)*.026+filtered(noise[:,channel],81)*.025
    mix*=(.85+.12*np.sin(t*.9)+.08*np.sin(t*2.3))[:,None]
    def place(sound,start,gain=1,pan=0):
        offset=int(start*sr);end=min(len(mix),offset+len(sound))
        if end<=offset:return
        mix[offset:end,0]+=sound[:end-offset]*gain*np.sqrt((1-pan)/2)
        mix[offset:end,1]+=sound[:end-offset]*gain*np.sqrt((1+pan)/2)
    for when in rng.uniform(0,duration,int(duration*12)):
        local=np.arange(int(.045*sr))/sr
        drop=filtered(rng.normal(0,1,len(local)),5)*np.exp(-local*120)*np.minimum(local/.001,1)
        place(drop,float(when),rng.uniform(.015,.04),rng.uniform(-.9,.9))
    traffic=np.sin(2*np.pi*(62*t+.7*t*t))*.007+filtered(rng.normal(0,1,len(t)),71)*.021
    swell=np.sin(np.pi*t/duration)**2
    mix[:,0]+=traffic*swell*(1-t/duration);mix[:,1]+=traffic*swell*t/duration

    def step(start,gain,pan=0):
        local=np.arange(int(.24*sr))/sr
        # Heel impact, later sole contact, scrape and wet pavement splash.
        body=np.sin(2*np.pi*(58*local+22*.025*(1-np.exp(-local/.025))))*np.exp(-local*28)
        tap=filtered(rng.normal(0,1,len(local)),7)*np.exp(-local*55)
        sole=filtered(rng.normal(0,1,len(local)),4)*np.exp(-np.maximum(local-.037,0)*65)*(local>.037)
        splash=(rng.normal(0,1,len(local))*.25+filtered(rng.normal(0,1,len(local)),13))
        splash*=np.minimum(local/.015,1)*np.exp(-local*22)
        sound=(body*.55+tap*.6+sole*.4+splash*.3)*np.minimum(local/.002,1)*np.minimum((.24-local)/.025,1)
        place(sound,start,gain,pan)
    def system_open(start,gain=1):
        # A single clean bell strike: sharp attack, stable pitch and a short metallic tail.
        local=np.arange(int(.72*sr))/sr
        bell=np.sin(2*np.pi*1568*local)*.075*np.exp(-local*5.8)
        bell+=np.sin(2*np.pi*3136*local)*.023*np.exp(-local*11)
        bell+=np.sin(2*np.pi*4380*local)*.009*np.exp(-local*17)
        bell*=np.minimum(local/.0015,1)*np.minimum((.72-local)/.08,1)
        place(bell,start,gain)
    if kind=='landing':step(.22,.35);step(.52,.07,.2)
    if kind=='system':
        system_open(.3,1.4)
        for when in (3.8,7.6,11.6):system_open(when,.45)
    if kind in ('scan','run'):
        rotor=(np.sin(2*np.pi*63*t)*.025+np.sin(2*np.pi*126*t)*.01)*(.8+.2*np.sin(2*np.pi*12*t))
        mix+=rotor[:,None]*(1 if kind=='scan' else .55)
    if kind=='scan':
        for when in (.2,2.15,4.1,6.05):
            local=np.arange(int(1.15*sr))/sr
            env=np.sin(np.pi*local/1.15)**2
            phase=2*np.pi*(210*local+330*local*local)
            beam=(np.sin(phase)*.065+np.sin(phase*2)*.016)*env
            beam+=filtered(rng.normal(0,1,len(local)),11)*env*.035
            place(beam,when,1.2,.15)
        # Two short lock-on pulses coincide with the identity warning.
        for when in (.5,.72):
            local=np.arange(int(.13*sr))/sr
            pulse=np.sin(2*np.pi*440*local)*.07*np.sin(np.pi*local/.13)**2
            place(pulse,when)
    if kind=='run':
        for num,when in enumerate(np.arange(.12,3.65,.34)):
            gain=.055+.018*num
            step(float(when),gain,(-.7+.09*num))
        for num,when in enumerate(np.arange(4.3,7.65,.255)):step(float(when),max(.07,.23-num*.012),.1+num*.045)
    if kind=='shelter':
        mix*=.35
        for num,when in enumerate((.15,.45,.76)):step(when,.12-num*.025,-.15)
        system_open(3.9,.7)
    fade=np.minimum(t/.06,1)*np.minimum((duration-t)/.12,1);mix*=fade[:,None]
    if kind=='shelter':mix*=np.minimum((duration-t)/2,1)[:,None]
    with wave.open(str(folder/'环境音效-v4.wav'),'wb') as w:
        w.setnchannels(2);w.setsampwidth(2);w.setframerate(sr);w.writeframes((np.clip(mix,-.9,.9)*32767).astype('<i2').tobytes())

def text_filter(text,filename,folder,size,x,y,color='white',begin=0,end=100):
    (folder/filename).write_text(text,encoding='utf-8')
    return f",drawtext=fontfile='{font}':textfile={filename}:fontsize={size}:fontcolor={color}:borderw=2:bordercolor=black:x={x}:y={y}:enable='between(t,{begin},{end})'"

def render(shot):
    folder=base/shot['folder'];folder.mkdir(exist_ok=True)
    make_sound(shot,folder)
    frames=int(shot['duration']*24)
    vf="scale=1280:720:flags=neighbor"
    if shot['sound']=='landing':vf+=',fade=t=in:st=0:d=0.35:color=white'
    if shot['sound']=='system':
        # Crisp UI is rendered as native graphic/text overlay, not baked into generated art.
        vf+=",drawbox=x=725:y=40:w=530:h=388:color=0x071A2D@0.86:t=fill:enable='gte(t,0.3)',drawbox=x=725:y=40:w=530:h=388:color=0x73DDD8@0.85:t=2:enable='gte(t,0.3)'"
        vf+=text_filter('万象系统 / MYRIAD SYSTEM','系统标题.txt',folder,23,'745','62','0x8FEFE6',.3,19)
        for num,(zh,en,start) in enumerate([('词域远征 · 词汇挑战赚金币','VOCABULARY RUNS / EARN COINS',3.8),('系统商店 · 装备、道具与技能','SYSTEM SHOP / GEAR AND SKILLS',7.6),('跨界支援 · 部署能力帮助伙伴','WORLD SUPPORT / HELP YOUR ALLIES',11.6)]):
            y=126+num*90
            vf+=text_filter(zh,f'功能{num}-中文.txt',folder,23,'745',str(y),'0xCDEDEB',start,19)
            vf+=text_filter(en,f'功能{num}-英文.txt',folder,18,'745',str(y+36),'0xB5C8D9',start,19)
    if shot['sound']=='scan':vf+=",drawbox=x=0:y='ih*0.47':w=iw:h=2:color=red@0.30:t=fill:enable='gte(t,1)'"
    for num,(begin,end,zh,en) in enumerate(shot['lines']):
        vf+=f",drawbox=x=0:y=ih-120:w=iw:h=120:color=black@0.36:t=fill:enable='between(t,{begin},{end})'"
        vf+=text_filter(zh,f'字幕{num}-中文.txt',folder,27,'(w-tw)/2','h-98','white',begin,end)
        vf+=text_filter(en,f'字幕{num}-英文.txt',folder,23,'(w-tw)/2','h-56','0xE7E9ED',begin,end)
    if shot['sound']=='shelter':vf+=',fade=t=out:st=9.2:d=0.8:color=black'
    target=folder/(shot['folder']+'-v4.mp4')
    subprocess.run([ffmpeg,'-y','-loglevel','error','-loop','1','-framerate','24','-i',str(next(base/shot['scene']/name for name in ('场景-v4.png','场景-v3.png','场景-v1.png') if (base/shot['scene']/name).exists())),'-i',str(folder/'环境音效-v4.wav'),'-vf',vf,'-t',str(shot['duration']),'-c:v','libx264','-crf','18','-pix_fmt','yuv420p','-c:a','aac','-b:a','160k','-movflags','+faststart',str(target)],cwd=folder,check=True)
    print('Rendered '+shot['folder'],flush=True)
    return target

if __name__=='__main__':
    outputs=[render(shot) for shot in shots]
    clips=[base/'第一段-通宵刷题/第一段CG-通宵刷题-v5-双语无提示音.mp4',base/'第二段-屏幕异变/第二段CG-屏幕异变-v1.mp4']+outputs
    playlist=base/'完整CG片段列表-v4.txt'
    playlist.write_text('\n'.join("file '"+str(file).replace('\\','/')+"'" for file in clips),encoding='utf-8')
    subprocess.run([ffmpeg,'-y','-loglevel','error','-f','concat','-safe','0','-i',str(playlist),'-c:v','copy','-c:a','aac','-b:a','160k','-movflags','+faststart',str(base/'开场CG-完整版-v4-身高比例修正.mp4')],check=True)
    (base/'开场CG-分镜-v4.json').write_text(json.dumps(shots,ensure_ascii=False,indent=2),encoding='utf-8')
    print('Rendered full opening CG, 67 seconds.',flush=True)
