from pathlib import Path
from PIL import Image, ImageOps, ImageDraw
import json

ROOT = Path(__file__).resolve().parent
OUT = ROOT.parents[1] / 'Assets/Sprites/TileMaps/Desert64'
COLORS = ['#FFE59A','#FFD16D','#F6BA50','#EAA03C','#EE963E','#E58032','#DB692D','#C8572D','#BA4E36','#A34440','#903B48','#7A354B','#69304B','#552A46','#43243E','#322035']
OUT.mkdir(parents=True, exist_ok=True)
individual = ROOT / 'IndividualTiles'
individual.mkdir(exist_ok=True)
source = Image.open(ROOT / 'source_cracks.png').convert('L')
base = Image.new('RGBA', (256,256))
overlay = Image.new('RGBA', (256,256))
preview = Image.new('RGB', (816,300), '#eee6d6')
draw = ImageDraw.Draw(preview)
draw.text((8,8), 'BASE                           TRANSPARENCIA                   COMBINACION', fill='#43243e')
for i,color in enumerate(COLORS):
    x,y=i%4,i//4
    flat=Image.new('RGBA',(64,64),color)
    crop=source.crop((round(x*source.width/4),round(y*source.height/4),round((x+1)*source.width/4),round((y+1)*source.height/4)))
    mask=ImageOps.invert(crop).point(lambda v: 0 if v<30 else v)
    mask=mask.crop(mask.getbbox())
    mask.thumbnail((56,56),Image.Resampling.LANCZOS)
    alpha=Image.new('L',(64,64))
    alpha.paste(mask,((64-mask.width)//2,(64-mask.height)//2))
    alpha=alpha.point(lambda v: 0 if v<12 else v)
    decal=Image.new('RGBA',(64,64),'#54263E')
    decal.putalpha(alpha)
    base.paste(flat,(x*64,y*64))
    overlay.paste(decal,(x*64,y*64))
    flat.save(individual/f'base_{i+1:02}.png')
    decal.save(individual/f'texture_{i+1:02}.png')
    checker=Image.new('RGBA',(64,64),'#cccccc')
    d=ImageDraw.Draw(checker)
    for yy in range(0,64,8):
        for xx in range(0,64,8):
            if (xx//8+yy//8)%2: d.rectangle((xx,yy,xx+7,yy+7),fill='#eeeeee')
    for panel,tile in enumerate((flat,Image.alpha_composite(checker,decal),Image.alpha_composite(Image.new('RGBA',(64,64),COLORS[i%8]),decal))):
        preview.paste(tile.convert('RGB'),(8+panel*272+x*64,32+y*64))
    assert len(flat.getcolors())==1
    assert alpha.getextrema()[0]==0 and alpha.getextrema()[1]>0
    assert alpha.getbbox()[0]>=4 and alpha.getbbox()[1]>=4
base.save(OUT/'Desert_BaseColors_64.png')
overlay.save(OUT/'Desert_TextureOverlay_64.png')
preview.resize((1632,600),Image.Resampling.NEAREST).save(ROOT/'preview.png')
for path in individual.glob('*.png'):
    assert Image.open(path).size==(64,64)
report={'tile_size':[64,64],'atlas_size':[256,256],'tiles_per_atlas':16,'base_uniform':True,'overlay_transparent':True,'colors':COLORS}
(ROOT/'validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report))
