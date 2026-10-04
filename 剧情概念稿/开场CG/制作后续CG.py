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
    duration=shot['duration'];kind=shot['sound'];rng=np.random.default_rng(360+shots.index(shot));t=np.arange(int(duration*sr))/sr
    noise=rng.normal(0,1,len(t));low=np.convolve(noise,np.ones(13)/13,mode='same')
    # Stereo rain and distant city machinery. No music, voices, keyboard or text cues.
    mix=np.column_stack((noise*.008+low*.027,np.roll(noise,int(.019*sr))*.008+np.roll(low,int(.031*sr))*.027))
    if kind in ('scan','run'):
        rotor=np.sin(2*np.pi*76*t)*(.026+.01*np.sin(2*np.pi*7*t))
        mix+=rotor[:,None]
    if kind=='scan':
        intensity=np.minimum(t/2,1)*np.minimum((duration-t)/.8,1)
        mix+=(np.sin(2*np.pi*(135*t+5*t*t))*.017*intensity)[:,None]
    def impact(start,gain):
        local=np.arange(int(.16*sr))/sr
        click=(rng.normal(0,.35,len(local))+.4*np.sin(2*np.pi*90*local))*np.exp(-local*35)*gain
        at=int(start*sr);end=min(len(mix),at+len(click));mix[at:end]+=click[:end-at,None]
    if kind=='landing':impact(.28,.16)
    if kind=='run':
        for when in np.arange(.2,4,.36):impact(float(when),.07)
        for when in np.arange(4.5,6.8,.25):impact(float(when),.06)
    if kind=='shelter':
        mix*=.6
        for when in (.2,.48,.74):impact(when,.045)
    fade=np.minimum(t/.12,1)*np.minimum((duration-t)/.3,1);mix*=fade[:,None]
    if kind=='shelter':mix*=np.minimum((duration-t)/2,1)[:,None]
    with wave.open(str(folder/'环境音效.wav'),'wb') as w:
        w.setnchannels(2);w.setsampwidth(2);w.setframerate(sr);w.writeframes((np.clip(mix,-.9,.9)*32767).astype('<i2').tobytes())

def text_filter(text,filename,folder,size,x,y,color='white',begin=0,end=100):
    (folder/filename).write_text(text,encoding='utf-8')
    return f",drawtext=fontfile='{font}':textfile={filename}:fontsize={size}:fontcolor={color}:borderw=2:bordercolor=black:x={x}:y={y}:enable='between(t,{begin},{end})'"

def render(shot):
    folder=base/shot['folder'];folder.mkdir(exist_ok=True)
    make_sound(shot,folder)
    frames=int(shot['duration']*24)
    vf=f"scale=640:360:flags=neighbor,zoompan=z='1+0.045*on/{frames-1}':x='iw*0.5-iw/zoom*0.5':y='ih*0.47-ih/zoom*0.47':d={frames}:s=640x360:fps=24,scale=1280:720:flags=neighbor"
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
    target=folder/(shot['folder']+'-v1.mp4')
    subprocess.run([ffmpeg,'-y','-loglevel','error','-i',str(base/shot['scene']/'场景-v1.png'),'-i',str(folder/'环境音效.wav'),'-vf',vf,'-t',str(shot['duration']),'-c:v','libx264','-crf','18','-pix_fmt','yuv420p','-c:a','aac','-b:a','160k','-movflags','+faststart',str(target)],cwd=folder,check=True)
    print('Rendered '+shot['folder'],flush=True)
    return target

if __name__=='__main__':
    outputs=[render(shot) for shot in shots]
    clips=[base/'第一段-通宵刷题/第一段CG-通宵刷题-v5-双语无提示音.mp4',base/'第二段-屏幕异变/第二段CG-屏幕异变-v1.mp4']+outputs
    playlist=base/'完整CG片段列表.txt'
    playlist.write_text('\n'.join("file '"+str(file).replace('\\','/')+"'" for file in clips),encoding='utf-8')
    subprocess.run([ffmpeg,'-y','-loglevel','error','-f','concat','-safe','0','-i',str(playlist),'-c:v','copy','-c:a','aac','-b:a','160k','-movflags','+faststart',str(base/'开场CG-完整版-v1.mp4')],check=True)
    (base/'开场CG-分镜及提示词.json').write_text(json.dumps(shots,ensure_ascii=False,indent=2),encoding='utf-8')
    print('Rendered full opening CG, 67 seconds.',flush=True)
