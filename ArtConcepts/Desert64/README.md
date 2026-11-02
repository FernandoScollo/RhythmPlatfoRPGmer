# Desert 64

Dos atlas de 256 × 256, cada uno con 16 tiles de exactamente 64 × 64, sin separación. PNG en Assets/Sprites/TileMaps/Desert64. Copias individuales en IndividualTiles.

BaseColors contiene colores planos opacos. TextureOverlay contiene solo grietas/detalles color ciruela y transparencia real. Son decals independientes, no una red continua de grietas. El damero solo aparece en preview.png.

Unity: importar como Sprite (2D and UI), Multiple, Pixels Per Unit 64, Full Rect, sin mipmaps, Compression None. Sprite Editor: Slice > Grid By Cell Size 64 × 64, Offset 0, Padding 0. Crear dos Tile Palettes con los sprites. Usar dos Tilemaps en la misma Grid, con las texturas en un Order in Layer superior, sin collider decorativo. Los tonos claros dan mayor contraste.

No se modificaron escenas ni se crearon Tile Palettes automáticamente. El arte fuente fue generado con la herramienta integrada de imágenes; build_tiles.py lo procesa con Pillow y verifica tamaños, uniformidad y alfa. source_cracks.png conserva el original y validation.json los resultados.
