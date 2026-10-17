from pathlib import Path
from PIL import Image
from pptx import Presentation
from pptx.util import Inches, Pt
from pptx.dml.color import RGBColor
from pptx.enum.text import PP_ALIGN, MSO_ANCHOR
from pptx.enum.shapes import MSO_SHAPE
from docx import Document
from docx.shared import Inches as DInches, Pt as DPt, RGBColor as DRGB
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT, WD_CELL_VERTICAL_ALIGNMENT
from docx.oxml import OxmlElement
from docx.oxml.ns import qn

ROOT = Path(r"C:\Users\Usuario\source\repos\Grupo-N-10---SistemaDeTiendaDeRopaOnline")
OUT_PPT = ROOT / "F&R_Presentacion_ICES.pptx"
OUT_DOC = ROOT / "F&R_Guion_y_Guia_de_Exposicion.docx"
TMP = ROOT / ".fyr_slide_assets"
TMP.mkdir(exist_ok=True)

screens = {
    "catalogo": Path(r"C:\Users\Usuario\AppData\Local\Temp\codex-clipboard-33c9fed4-eb5c-4e56-9392-cd66dd5199bb.png"),
    "detalle": Path(r"C:\Users\Usuario\AppData\Local\Temp\codex-clipboard-a2ab5758-f66a-4bb4-bd7e-2f4f93d4142b.png"),
    "carrito": Path(r"C:\Users\Usuario\AppData\Local\Temp\codex-clipboard-4a71b1bf-73ef-4d16-aa51-9370884b4bb0.png"),
    "compras": Path(r"C:\Users\Usuario\AppData\Local\Temp\codex-clipboard-e398a993-6cf5-4f02-bfc7-a66a3f2f5f6b.png"),
    "admin": Path(r"C:\Users\Usuario\AppData\Local\Temp\codex-clipboard-473ded1d-a115-4ec6-93d6-2fe1085a6a04.png"),
}
# Crop only browser chrome/taskbar. Screens remain authentic application evidence.
crop_boxes = {
    "catalogo": (0, 180, 1898, 1017),
    "detalle": (200, 180, 1898, 1017),
    "carrito": (330, 180, 1560, 880),
    "compras": (450, 250, 1450, 760),
    "admin": (200, 180, 1700, 1017),
}
for name, src in screens.items():
    im = Image.open(src).convert("RGB")
    im.crop(crop_boxes[name]).save(TMP / f"{name}.jpg", quality=92)

# Presentation styling echoes the reference deck's restrained palette and warm accent.
prs = Presentation()
prs.slide_width, prs.slide_height = Inches(13.333), Inches(7.5)
blank = prs.slide_layouts[6]
INK = RGBColor(22, 22, 22)
PAPER = RGBColor(248, 247, 244)
WHITE = RGBColor(255, 255, 255)
ORANGE = RGBColor(221, 116, 50)
MUTED = RGBColor(105, 106, 108)
PALE = RGBColor(236, 232, 225)
BLUE = RGBColor(58, 83, 255)
FONT = "Aptos"

def textbox(slide, x, y, w, h, text, size=18, color=INK, bold=False, font=FONT, align=None):
    box = slide.shapes.add_textbox(Inches(x), Inches(y), Inches(w), Inches(h))
    tf = box.text_frame
    tf.clear()
    tf.word_wrap = True
    tf.margin_left = Inches(.02); tf.margin_right = Inches(.02)
    tf.margin_top = Inches(.01); tf.margin_bottom = Inches(.01)
    for ix, line in enumerate(text.split("\n")):
        p = tf.paragraphs[0] if ix == 0 else tf.add_paragraph()
        p.text = line
        p.font.name = font; p.font.size = Pt(size); p.font.bold = bold; p.font.color.rgb = color
        if align is not None: p.alignment = align
        p.space_after = Pt(3)
    return box

def rect(slide, x, y, w, h, fill, line=None, radius=False):
    shape = slide.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE if radius else MSO_SHAPE.RECTANGLE,
                                   Inches(x), Inches(y), Inches(w), Inches(h))
    shape.fill.solid(); shape.fill.fore_color.rgb = fill
    shape.line.fill.background() if line is None else None
    if line:
        shape.line.color.rgb = line
    if radius:
        shape.adjustments[0] = .08
    return shape

def base(title, section="F&R  |  ICES", time=""):
    s = prs.slides.add_slide(blank)
    s.background.fill.solid(); s.background.fill.fore_color.rgb = PAPER
    textbox(s, .55, .25, 9.5, .32, section.upper(), 10, ORANGE, True)
    textbox(s, .55, .68, 12.1, .65, title, 28, INK, True)
    rect(s, .55, 1.42, .42, .045, ORANGE)
    textbox(s, .55, 7.12, 9.2, .2, "F&R  ·  Sistema de tienda de ropa online", 9, MUTED)
    if time: textbox(s, 11.6, 7.08, 1.15, .25, time, 10, MUTED, True, align=PP_ALIGN.RIGHT)
    return s

def notes(slide, text):
    slide.notes_slide.notes_text_frame.text = text

def add_image(slide, name, x, y, w, h):
    path = TMP / f"{name}.jpg"
    im = Image.open(path)
    aspect = im.width / im.height
    frame = w / h
    if aspect >= frame:
        iw, ih = w, w / aspect
        ix, iy = x, y + (h - ih) / 2
    else:
        ih, iw = h, h * aspect
        ix, iy = x + (w - iw) / 2, y
    slide.shapes.add_picture(str(path), Inches(ix), Inches(iy), Inches(iw), Inches(ih))

def bullet_list(slide, x, y, w, items, size=17, color=INK, gap=.55):
    for i, item in enumerate(items):
        yy = y + i * gap
        rect(slide, x, yy + .11, .10, .10, ORANGE, radius=True)
        textbox(slide, x + .22, yy, w - .22, gap - .03, item, size, color)

# 1. Opening
s = prs.slides.add_slide(blank); s.background.fill.solid(); s.background.fill.fore_color.rgb = INK
rect(s, 0, 0, .16, 7.5, ORANGE)
textbox(s, .85, .7, 4, .35, "ICES  ·  PROYECTO ACADÉMICO", 12, ORANGE, True)
textbox(s, .85, 1.5, 5.6, 1.25, "F&R", 56, WHITE, True)
textbox(s, .88, 2.8, 5.4, 1.0, "Una tienda de ropa online\ncon gestión integrada", 27, WHITE, True)
textbox(s, .88, 5.95, 5.2, .7, "De descubrir una prenda a administrar el catálogo", 17, RGBColor(215,215,215))
add_image(s, "catalogo", 6.65, 1.1, 5.95, 3.25)
textbox(s, .88, 6.95, 5.5, .24, "Exposición formal  ·  20 minutos", 11, RGBColor(190,190,190))
notes(s, "TIEMPO 1:00. Apertura: presentar F&R como proyecto académico para una tienda de indumentaria. Idea: la experiencia de compra y la gestión del comercio están conectadas. Mostrar la portada con esta diapositiva; pasar a la siguiente con la pregunta disparadora.")

# 2. Problem and opportunity
s = base("¿Cómo vender online sin perder el control de la tienda?", "01  ·  Problema y oportunidad", "1:30")
textbox(s, .65, 1.78, 5.0, .45, "OPORTUNIDAD", 12, ORANGE, True)
textbox(s, .65, 2.2, 5.0, 1.3, "Llegar a más clientes y ofrecer un catálogo disponible en la web.", 25, INK, True)
textbox(s, 6.6, 1.78, 5.8, .45, "FRICCIONES QUE F&R ABORDA", 12, ORANGE, True)
bullet_list(s, 6.6, 2.22, 5.8, ["Productos dispersos o difíciles de consultar", "Pedidos y stock que requieren seguimiento", "Gestión del catálogo y ventas con tareas manuales"], 18, gap=.72)
rect(s, .65, 5.25, 11.85, 1.0, INK, radius=True)
textbox(s, .9, 5.55, 11.2, .5, "La propuesta: reunir compra online y operaciones básicas en un mismo sistema.", 20, WHITE, True)
notes(s, "TIEMPO 1:30. Pregunta disparadora: ¿cómo puede una tienda de ropa ampliar su alcance sin que productos, pedidos y stock queden desorganizados? La documentación plantea la falta de presencia digital como problema y menciona el crecimiento del comercio electrónico como oportunidad. Aclarar que la necesidad es el punto de partida del proyecto, no un estudio cuantitativo de mercado. Transición: presentar la solución.")

# 3. Solution and value
s = base("Una experiencia de compra conectada con la gestión", "02  ·  Solución", "1:00")
add_image(s, "catalogo", .65, 2.05, 6.0, 3.25)
textbox(s, 7.15, 1.85, 5.2, .45, "PARA QUIEN COMPRA", 12, ORANGE, True)
bullet_list(s, 7.15, 2.25, 5.1, ["Explorar y filtrar prendas", "Elegir talle, color y cantidad", "Armar un carrito y consultar compras"], 17, gap=.65)
textbox(s, 7.15, 4.55, 5.1, .4, "PARA QUIEN GESTIONA", 12, ORANGE, True)
textbox(s, 7.15, 4.95, 5.1, 1.1, "Administrar productos y mantener el catálogo actualizado.", 21, INK, True)
notes(s, "TIEMPO 1:00. F&R conecta dos recorridos: el cliente consulta y compra; el administrador mantiene el catálogo. Mostrar la captura real del catálogo como evidencia. No prometer integraciones externas, personalización por comercio ni procesamiento bancario. Transición: explicar cómo viaja la operación.")

# 4. How it works / architecture
s = base("El recorrido del dato, desde la pantalla hasta la base", "03  ·  Cómo funciona", "2:00")
boxes = [("PERSONA", "Cliente / administrador"), ("FRFront", "MVC · Razor · sesión"), ("FYR-API", "Minimal APIs · servicios"), ("DATOS", "EF Core · SQL Server")]
for i, (head, sub) in enumerate(boxes):
    x = .65 + i * 3.1
    rect(s, x, 2.15, 2.55, 1.45, WHITE, line=PALE, radius=True)
    textbox(s, x+.16, 2.43, 2.2, .35, head, 15, ORANGE, True, align=PP_ALIGN.CENTER)
    textbox(s, x+.15, 2.91, 2.25, .5, sub, 14, INK, True, align=PP_ALIGN.CENTER)
    if i < 3: textbox(s, x+2.62, 2.72, .42, .3, "→", 20, ORANGE, True, align=PP_ALIGN.CENTER)
textbox(s, .75, 4.25, 5.5, .42, "EJEMPLO: CONSULTA DE PRODUCTO", 12, ORANGE, True)
textbox(s, .75, 4.7, 5.5, 1.0, "La vista pide el catálogo; la API consulta los datos y devuelve productos para mostrar.", 19, INK, True)
textbox(s, 6.8, 4.25, 5.6, .42, "PERSISTENCIA DE LA COMPRA", 12, ORANGE, True)
textbox(s, 6.8, 4.7, 5.5, 1.0, "El frontend envía el pedido; la API lo registra con sus detalles en SQL Server.", 19, INK, True)
textbox(s, .75, 6.1, 11.7, .42, ".NET 10  ·  ASP.NET Core MVC  ·  Minimal APIs  ·  EF Core  ·  SQL Server", 14, MUTED, True)
notes(s, "TIEMPO 2:00. Describir el camino sencillo: interfaz MVC/Razor, comunicación HTTP con la API, lógica/servicios y persistencia con Entity Framework Core en SQL Server. Tecnologías según los proyectos y análisis técnico. Si se pregunta por código, mostrar brevemente HomeController.Catalogo, ProductoController o PedidoEndpoints y PedidoService, sin recorrer archivos extensos. La API se desarrolla con Minimal APIs. Transición: mostrar cómo se expresan las funciones en requisitos y casos de uso.")

# 5. Functional req/use cases 1-3
s = base("Tres funciones clave y sus casos de uso", "04  ·  Requerimientos funcionales", "2:00")
rows = [
    ("RF 9.1 · Gestión de productos", "CU-P1 propuesto · Administrar producto\nAdministrador · crear / editar / eliminar", "ProductoController · vistas Crear, Modificar, Index\nPantalla: gestión de productos · resultado: catálogo actualizado"),
    ("RF 9.2 · Consulta y filtros", "CU-P2 propuesto · Buscar prendas\nCliente · filtrar categoría, color o talle", "HomeController.Catalogo · _CatalogoProductos\nPantalla: catálogo · resultado: lista filtrada"),
    ("RF 9.3 · Acceso por rol", "CU-P3 propuesto · Iniciar sesión y navegar\nAdministrador / empleado / cliente", "AccountController · Rol.cs\nPantalla: ingreso y menú por rol · acceso de interfaz diferenciado"),
]
for i, (a,b,c) in enumerate(rows):
    y=1.82+i*1.57
    rect(s,.65,y,12.0,1.3,WHITE,line=PALE,radius=True)
    textbox(s,.88,y+.18,3.15,.94,a,16,INK,True)
    textbox(s,4.15,y+.16,3.65,1.0,b,13,INK)
    textbox(s,8.0,y+.16,4.3,1.0,c,12,MUTED)
textbox(s,.72,6.62,12,.28,"Códigos 9.1–9.3 conservan la numeración de cumplimiento del PDF. CU-P1–P5 son propuestas, porque la documentación no identifica CUs formales.",10,MUTED)
notes(s, "TIEMPO 2:00. Presentar cada fila como necesidad → acción del actor → evidencia en código y pantalla. RF 9.1 recoge la gestión de productos; resultado observable: producto aparece o cambia en la lista/catálogo. RF 9.2 muestra filtro y resultado. RF 9.3 requiere una precisión: la interfaz diferencia roles, pero la autorización de API no está aplicada de forma uniforme según el análisis del código; describirlo como navegación diferenciada, no como seguridad completa. Los casos de uso aquí y en la siguiente diapositiva son propuestas de exposición, pues el PDF solo enumera requerimientos. Las pantallas y archivos citados existen en el repositorio.")

# 6. Functional req/use cases 4-5
s = base("La compra también deja un registro consultable", "04  ·  Requerimientos funcionales", "2:00")
rows = [
    ("RF 9.4 · Carrito y proceso de compra", "CU-P4 propuesto · Preparar y confirmar pedido\nCliente · elegir artículo, cantidad y envío", "CarritoController · CheckoutController\nPantalla: carrito / checkout · resultado: pedido registrado\nPago simulado, sin pasarela bancaria"),
    ("RF 9.5 · Compras y seguimiento", "CU-P5 propuesto · Consultar compra\nCliente · revisar estado y detalle", "ClienteController.MisCompras · PedidoEndpoints\nPantalla: Mis compras · resultado: pedido, fecha, estado y total"),
]
for i,(a,b,c) in enumerate(rows):
    y=2.0+i*1.85
    rect(s,.65,y,12.0,1.55,WHITE,line=PALE,radius=True)
    textbox(s,.9,y+.2,3.05,1.1,a,16,INK,True)
    textbox(s,4.1,y+.2,3.55,1.1,b,14,INK)
    textbox(s,7.95,y+.2,4.35,1.1,c,13,MUTED)
textbox(s,.9,6.1,11.7,.48,"El registro del pedido existe; el cobro real y el seguimiento logístico externo no están demostrados.",15,ORANGE,True)
notes(s, "TIEMPO 2:00. RF 9.4 enlaza el carrito con la creación del pedido; explicar que el pago actual simula la operación y no conecta a una pasarela. RF 9.5 enlaza el historial con la consulta de estado/detalle. Código: CarritoController, CheckoutController, ClienteController.MisCompras y PedidoEndpoints. Mostrar la evidencia en vivo en los siguientes minutos. Los CU-P4 y CU-P5 se proponen porque el documento no proporciona identificadores o fichas de casos de uso.")

# 7. Demo
s = base("Demostración integrada: descubrir, elegir y consultar", "05  ·  Demo formal en navegador", "5:00")
add_image(s,"detalle",.62,1.82,3.95,2.12)
add_image(s,"carrito",4.7,1.82,3.95,2.12)
add_image(s,"compras",8.78,1.82,3.95,2.12)
textbox(s,.65,4.15,3.85,.36,"1  Catálogo y detalle",14,ORANGE,True)
textbox(s,.65,4.53,3.85,1.0,"Filtrar y elegir una prenda. Señalar talle, color, cantidad y stock.",14,INK)
textbox(s,4.72,4.15,3.85,.36,"2  Carrito",14,ORANGE,True)
textbox(s,4.72,4.53,3.85,1.0,"Revisar artículo, cantidad y total. El cobro actual es simulado.",14,INK)
textbox(s,8.8,4.15,3.85,.36,"3  Mis compras",14,ORANGE,True)
textbox(s,8.8,4.53,3.85,1.0,"Mostrar el registro de compra y su estado. No implica seguimiento logístico externo.",14,INK)
textbox(s,.65,6.55,12,.3,"Pasar al navegador después de la diapositiva 6. Volver aquí al cerrar el historial.",11,MUTED,True)
notes(s, "TIEMPO 5:00 INCLUYE TRANSICIONES. En esta diapositiva pasar del PowerPoint al navegador. Orden: (1) abrir catálogo; usar categoría/color/talle y aplicar filtros; (2) abrir un producto; mostrar talle, color, stock, cantidad y botón de agregar; (3) abrir carrito, señalar artículo, control de cantidad y total; (4) solo si está preparado un usuario de demostración y el flujo es estable, finalizar una compra de prueba con datos ficticios, sin datos reales de tarjeta; (5) abrir Mis compras y señalar número/estado/total/detalle. Aclarar que el checkout es simulación. Volver a la presentación al terminar. Preparar previamente backend, frontend, base con datos de demostración y una sesión de cliente válida. No inventar credenciales: usar las que el estudiante ya tenga y comprobarlas antes de exponer. Si falla el cierre, mostrar carrito e historial ya existente en la captura como referencia y no improvisar una compra. Código opcional: HomeController.Catalogo, CarritoController y CheckoutController, solo si lo pregunta el profesor.")

# 8. Admin demo and quality
s = base("Gestión visible y calidad con alcance claro", "06  ·  Funciones internas y calidad", "2:30")
add_image(s,"admin",.62,1.82,6.05,3.38)
textbox(s,6.95,1.84,5.5,.35,"CINCO ATRIBUTOS CONTEMPLADOS",12,ORANGE,True)
quality = [
    "Seguridad y privacidad: BCrypt, HTTPS redirect y antiforgery; protección integral de API/datos no verificada.",
    "Confiabilidad: health check y persistencia; disponibilidad y transacciones atómicas no comprobadas.",
    "Usabilidad/adaptabilidad: Bootstrap y grillas responsivas; accesibilidad requiere auditoría.",
    "Modularidad y mantenimiento: frontend MVC, API, servicios, repositorios y DTOs separados.",
    "Persistencia/integración: API y EF Core gestionan datos en SQL Server.",
]
for i,t in enumerate(quality):
    yy=2.28+i*.79
    rect(s,6.95,yy,.28,.28,ORANGE,radius=True)
    textbox(s,7.4,yy-.04,5.0,.62,t,13,INK)
textbox(s,.65,5.5,5.9,.8,"En vivo: panel de productos → buscar un artículo → mostrar precio y stock. Evitar guardar cambios durante la exposición.",15,INK)
textbox(s,.65,6.42,11.9,.35,"Rendimiento, disponibilidad, copias de seguridad y seguridad completa necesitan pruebas o implementación adicional.",12,ORANGE,True)
notes(s, "TIEMPO 2:30. Mostrar brevemente la pantalla real de gestión de productos (aprox. 45 segundos), sin modificar datos en el vivo. Explicar los cinco NFR como ‘contemplados’ con su evidencia, no como certificaciones o cumplimiento medido. Seguridad: repositorio usa BCrypt y hay HTTPS redirection y tokens declarados/generados, pero el backend no demuestra autorización uniforme; no calificar el producto como seguro integralmente. Usabilidad: Bootstrap responsivo sí aparece; accesibilidad necesita revisión. Modularidad/mantenibilidad: separación FRFront/FYR-API y servicios/repositorios/DTOs. Persistencia: EF Core y SQL Server. El PDF pide además HTTPS, MFA, alto rendimiento, disponibilidad, backups, integración oficial de pagos; no afirmar que se cumplan sin evidencia. Para mostrar código: AuthService BCrypt, Program.cs y endpoints puntuales.")

# 9. Outcomes and future
s = base("Lo logrado y lo que sigue", "07  ·  Cierre", "1:30")
textbox(s,.75,1.9,5.4,.4,"LOGROS DEMOSTRABLES",12,ORANGE,True)
bullet_list(s,.75,2.32,5.6,["Catálogo con consulta y filtros", "Gestión de productos y stock", "Carrito, pedidos e historial de compras"],17,gap=.72)
textbox(s,6.95,1.9,5.4,.4,"PRÓXIMAS MEJORAS",12,ORANGE,True)
bullet_list(s,6.95,2.32,5.6,["Pasarela de pago real y transacciones robustas", "Aislamiento multiempresa y autorización API", "Tabla de talles y varias imágenes por producto"],16,gap=.72)
rect(s,.75,5.35,11.8,.95,INK,radius=True)
textbox(s,1.0,5.62,11.2,.45,"F&R ya resuelve un recorrido de compra y gestión; el siguiente paso es robustecer seguridad e integración.",18,WHITE,True)
notes(s,"TIEMPO 1:30. Resumir logros apoyados por código y capturas: catálogo filtrable, gestión de producto, carrito, pedido e historial. Mejoras documentadas: talles/medidas y varias fotos. Otras mejoras técnicas respaldadas por brechas del análisis: proveedor real de pago, robustecer autorización y aislamiento multiempresa, transacciones/idempotencia y pruebas. Concluir con una oportunidad concreta: el sistema ya demuestra el flujo base de tienda, y la evolución debe priorizar seguridad y confiabilidad antes de escalarlo como servicio comercial. Agradecer y abrir preguntas. La formal termina aquí; restan hasta 10 minutos opcionales.")

# 10. closing / time check
s = prs.slides.add_slide(blank); s.background.fill.solid(); s.background.fill.fore_color.rgb=INK
rect(s,0,0,.16,7.5,ORANGE)
textbox(s,.9,1.2,10.6,.45,"F&R  ·  ICES",13,ORANGE,True)
textbox(s,.9,2.0,10.8,1.2,"Una tienda online\ncon gestión conectada",36,WHITE,True)
textbox(s,.9,4.8,9.8,.7,"Gracias. ¿Qué parte del recorrido quieren que revisemos?",21,WHITE)
textbox(s,.9,6.85,8,.25,"20 minutos formales  ·  hasta 10 minutos para ampliar y responder",11,RGBColor(190,190,190))
notes(s,"TIEMPO 1:30 (incluido en el total de 20 minutos). Cerrar la exposición y habilitar preguntas. Los 10 minutos posteriores son margen independiente, no parte del discurso formal obligatorio.")

prs.core_properties.title = "F&R | Presentación del sistema de tienda de ropa online"
prs.core_properties.subject = "Exposición ICES de 20 minutos con demostración web integrada"
prs.core_properties.author = "F&R - Proyecto académico ICES"
prs.save(OUT_PPT)

# Complementary oral guide, evidence matrix and optional 10-minute demo.
doc = Document()
sec = doc.sections[0]; sec.top_margin=DInches(.65); sec.bottom_margin=DInches(.65); sec.left_margin=DInches(.75); sec.right_margin=DInches(.75)
styles=doc.styles
styles['Normal'].font.name='Aptos'; styles['Normal'].font.size=DPt(10.5); styles['Normal'].font.color.rgb=DRGB(40,40,40)
for nm,sz,col in [('Title',25,'161616'),('Heading 1',17,'161616'),('Heading 2',12,'DD7432')]:
    styles[nm].font.name='Aptos Display' if nm!='Normal' else 'Aptos'; styles[nm].font.size=DPt(sz); styles[nm].font.bold=True; styles[nm].font.color.rgb=DRGB.from_string(col)
# Remove Word's built-in blue paragraph border from the Title style.
title_ppr = styles['Title']._element.pPr
if title_ppr is not None:
    border = title_ppr.find(qn('w:pBdr'))
    if border is not None: title_ppr.remove(border)
title=doc.add_paragraph(style='Title'); title.add_run("Guion de exposición de F&R")
sub=doc.add_paragraph(); sub.add_run("Presentación ICES · 20 minutos formales + 10 minutos opcionales").bold=True
doc.add_paragraph("Este documento acompaña el PowerPoint y permite explicar F&R con naturalidad. La ruta formal sigue el orden problema u oportunidad, solución, funcionamiento, funciones, calidad y cierre. La demostración del navegador está integrada en el tiempo de exposición. Las capturas recibidas son evidencia de pantallas reales; los casos de uso de la matriz se proponen para organizar la exposición porque el PDF no los identifica formalmente.")

def heading(txt, level=1): doc.add_heading(txt,level=level)
def para(label, txt):
    p=doc.add_paragraph(); r=p.add_run(label+" "); r.bold=True; p.add_run(txt); return p

heading("Guion por diapositiva")
scripts=[
 ("1. F&R", "1:00", "Presento F&R, un proyecto académico de ICES para una tienda de indumentaria. Integra la experiencia de compra con herramientas para gestionar el catálogo.", "Portada con la captura del catálogo.", "Pasar a la pregunta: ¿qué necesidad intenta resolver?"),
 ("2. Problema y oportunidad", "1:30", "La documentación parte de una tienda que quiere ampliar su alcance con presencia web, pero necesita organizar productos, pedidos y stock. F&R responde a esa oportunidad con una plataforma para consultar y comprar prendas.", "Solo diapositiva. No afirmar cifras de mercado.", "Presentar la solución concreta."),
 ("3. Solución", "1:00", "Para el cliente, el flujo empieza en el catálogo y continúa con selección y carrito. Para quien administra, hay una pantalla de gestión que mantiene los productos. La captura muestra la interfaz real.", "Captura de catálogo; indicar que la administración se ve en la demo posterior.", "Explicar qué componentes hacen posible ese recorrido."),
 ("4. Cómo funciona", "2:00", "La vista MVC/Razor muestra la interfaz. El frontend se comunica por HTTP con FYR-API, que organiza las operaciones mediante endpoints, servicios y repositorios. Entity Framework Core trabaja con SQL Server para persistir datos. Como ejemplo, el catálogo se consulta desde la API y se representa en el frontend.", "Diagrama de arquitectura. Código opcional: HomeController.Catalogo o PedidoEndpoints/PedidoService.", "Vincular arquitectura con requisitos verificables."),
 ("5. Funciones y casos 1–3", "2:00", "Relaciono tres requerimientos del documento con actores y pantallas. El administrador gestiona productos; el cliente busca y filtra; el usuario inicia sesión y ve una navegación acorde al rol. Los casos de uso se proponen para esta presentación. En acceso por rol, aclaro que la separación de menús no prueba autorización completa en la API.", "Capturas/tabla. La demo de catálogo y productos viene después.", "Seguir con compra e historial."),
 ("6. Funciones y casos 4–5", "2:00", "El cliente agrega artículos, ajusta cantidades y genera un pedido. Después puede consultar la compra y sus datos. El pago actual es simulado, por eso no lo presento como integración bancaria. Los estados que muestra la interfaz no prueban seguimiento con una empresa logística externa.", "Capturas del carrito e historial.", "Pasar del esquema a la web para comprobarlo."),
 ("7. Demo formal", "5:00", "Muestro el recorrido en vivo. Primero filtro el catálogo, después entro al detalle y señalo talle, color, cantidad y stock. Agrego al carrito, verifico total y, si el entorno está listo, finalizo una compra de prueba y abro Mis compras. Cierro aclarando que el cobro es simulado.", "Pasar de la diapositiva al navegador. Usar sesión de cliente y datos ficticios preparados previamente. Volver al PowerPoint al terminar.", "Mostrar la gestión interna y luego explicar los atributos de calidad."),
 ("8. Gestión y calidad", "2:30", "En el panel se ve gestión del catálogo. En calidad, distingo evidencia de promesa: BCrypt, HTTPS redirect y antiforgery aparecen en el código, pero la protección completa de la API no está confirmada. Bootstrap favorece la adaptación visual; accesibilidad requiere prueba. La separación por componentes y capas ayuda al mantenimiento. La persistencia usa API, EF Core y SQL Server. Rendimiento, backups y disponibilidad requieren trabajo y medición.", "45 segundos de pantalla de administración; después slide. Opcional: AuthService y Program.cs.", "Cerrar con resultados alcanzados y evolución futura."),
 ("9. Logros y mejoras", "1:30", "Ya se puede demostrar catálogo filtrable, gestión de productos, carrito, pedido e historial. La documentación propone tabla de talles y más imágenes. También conviene completar pago real, autorización, aislamiento multiempresa, transacciones y pruebas. El proyecto resuelve un recorrido base y deja clara una agenda técnica para hacerlo más confiable.", "PowerPoint. Preparar transición a preguntas.", "Agradecer y habilitar preguntas."),
 ("10. Cierre", "1:30", "Agradezco y pregunto qué parte del recorrido quieren revisar. La exposición formal termina aquí; los diez minutos restantes quedan para ampliar la demo y responder.", "PowerPoint; luego navegador o código según preguntas.", "Fin de la exposición formal."),
]
for name,t,say,show,link in scripts:
    heading(name,2); para("Tiempo",t); para("Idea para decir",say); para("Qué mostrar",show); para("Enlace",link)

heading("Matriz resumida de contraste")
doc.add_paragraph("La comparación usa el PDF de requerimientos (incluye en sus páginas 11–13 un resumen de cumplimiento), el análisis técnico adjunto y el código del repositorio disponible para esta exposición.")
matrix=[
 ("Catálogo y productos", "RF y cumplimiento descritos (PDF 7, 11)", "Controllers y vistas de catálogo/productos disponibles en el repositorio", "Coincidencia sustancial; mostrar gestión y filtro."),
 ("Carrito / pedido / pago", "Carrito, checkout y pago; error claro ante rechazo (PDF 8, 11)", "Carrito y registro de pedido descritos; checkout simulado, sin pasarela", "No llamar pago real al flujo. Validaciones/transacciones tienen límites."),
 ("Roles y permisos", "Tres roles y acceso restringido por empresa/rol (PDF 6–7, 11)", "Roles en frontend y sesión; análisis no encuentra autorización uniforme en API", "Mostrar menús diferenciados, aclarar brecha de autorización."),
 ("Multiempresa", "Aislamiento por EmpresaId (PDF 5–6)", "Modelo/campos presentes; filtros obligatorios no demostrados", "Diseño previsto; aislamiento aún no confirmado."),
 ("Seguridad y recuperación", "HTTPS, segundo factor opcional, defensa web y enlaces de recuperación (PDF 9–10)", "BCrypt y mecanismos parciales; recuperación por diccionario estático según análisis; no MFA demostrado", "No declarar seguridad completa ni recuperación acorde al documento."),
 ("Calidad", "Pide rendimiento <3 s, concurrencia, disponibilidad, accesibilidad y backups (PDF 9)", "Bootstrap responsivo, capas modulares, persistencia y validaciones descritas", "Hay evidencia parcial de calidad; métricas/pruebas faltantes."),
 ("Casos de uso", "Requerimientos funcionales; no se hallaron fichas CU identificadas", "Rutas, controladores, actores y vistas permiten proponer casos", "CU-P1 a CU-P5 son códigos propuestos, no IDs originales."),
 ("Mejoras explícitas", "Talles/medidas y varias imágenes en lista futura (PDF 14)", "No presentadas como funciones actuales", "Usar en el cierre como trabajo futuro."),
]
table=doc.add_table(rows=1, cols=4); table.alignment=WD_TABLE_ALIGNMENT.CENTER; table.style='Table Grid'
for cell,txt in zip(table.rows[0].cells,["Tema","Documentación","Análisis técnico / código","Decisión para la exposición"]):
    cell.text=txt
    shd=OxmlElement('w:shd'); shd.set(qn('w:fill'),'161616'); cell._tc.get_or_add_tcPr().append(shd)
    for p in cell.paragraphs:
        for r in p.runs: r.font.color.rgb=DRGB(255,255,255); r.font.bold=True
for row in matrix:
    cells=table.add_row().cells
    for cell,txt in zip(cells,row): cell.text=txt
for row_index, row in enumerate(table.rows):
    for cell in row.cells:
        cell.vertical_alignment=WD_CELL_VERTICAL_ALIGNMENT.CENTER
        if row_index > 0 and row_index % 2 == 0:
            shd=OxmlElement('w:shd'); shd.set(qn('w:fill'),'F4EFE8'); cell._tc.get_or_add_tcPr().append(shd)
        for p in cell.paragraphs:
            for r in p.runs: r.font.size=DPt(8.2)

heading("Cinco requerimientos funcionales y casos de uso")
func=[
 ("RF 9.1 · Gestión de productos", "CU-P1 (propuesto): Administrar producto. Actor: Administrador. Flujo: abrir gestión, crear/editar/eliminar y guardar. Resultado: ficha reflejada en gestión/catálogo. Código: ProductoController; vistas Crear, Modificar, Index. Pantalla: gestión de productos."),
 ("RF 9.2 · Consulta y filtrado", "CU-P2 (propuesto): Buscar prendas. Actor: Cliente. Flujo: abrir catálogo, elegir categoría/color/talle, aplicar. Resultado: lista filtrada o mensaje sin coincidencias. Código: HomeController.Catalogo y _CatalogoProductos. Pantalla: catálogo."),
 ("RF 9.3 · Acceso según rol", "CU-P3 (propuesto): Iniciar sesión y navegar. Actor: Administrador, empleado o cliente. Flujo: iniciar sesión, identificar rol y abrir menú correspondiente. Resultado: navegación distinta en frontend; autorización efectiva de API no uniforme según el análisis. Código: AccountController y Rol.cs. Pantalla: inicio de sesión/menú."),
 ("RF 9.4 · Carrito y compra", "CU-P4 (propuesto): Preparar y confirmar pedido. Actor: Cliente. Flujo: elegir producto, variante/cantidad, agregar al carrito y enviar pedido. Resultado: pedido registrado; pago simulado, no pasarela real. Código: CarritoController y CheckoutController. Pantalla: detalle, carrito y checkout."),
 ("RF 9.5 · Compras y estado", "CU-P5 (propuesto): Consultar compra. Actor: Cliente. Flujo: abrir Mis compras y ver pedido/detalle. Resultado: fecha, estado, total y artículos según pantalla. Código: ClienteController.MisCompras y PedidoEndpoints. Pantalla: Mis compras."),
]
for a,b in func: para(a,b)

heading("Cinco requerimientos no funcionales contemplados")
nfr=[
 ("Seguridad y privacidad (apartado 10, parcial)", "BCrypt para contraseñas, HTTPS redirection y antiforgery en acciones observadas. El registro de JWT no prueba protección de todos los endpoints; privacidad integral de datos y autorización API requieren verificación."),
 ("Confiabilidad (apartado 10, parcial)", "Hay health check y persistencia; no se confirman disponibilidad sostenida, transacción atómica ni idempotencia de una venta."),
 ("Usabilidad y adaptabilidad (PDF 10.1, parcial)", "Bootstrap y grillas responsivas sustentan el uso en distintos tamaños. Accesibilidad requiere pruebas con teclado/lector de pantalla y revisión de contraste."),
 ("Modularidad y mantenimiento (PDF 10.2 y 10.3)", "FRFront separado de FYR-API; endpoints, servicios, DTOs, repositorios y entidades separan responsabilidades. No se detectó cobertura automatizada mantenida."),
 ("Persistencia, integración y validaciones (PDF 10.4 y 10.5)", "Frontend usa HttpClient; API, EF Core y SQL Server gestionan datos, y hay validaciones con mensajes. La integración bancaria oficial y la disponibilidad/backups no están demostradas."),
]
for a,b in nfr: para(a,b)
doc.add_paragraph("No se presentan como cumplidos los objetivos documentales de carga menor a 3 segundos, alta concurrencia, disponibilidad, copias diarias, MFA o accesibilidad certificada: requerirían implementación, mediciones o pruebas adicionales.")

heading("Demostración adicional de 10 minutos")
demo=[
 ("0:00–2:00 · Gestión de catálogo", "Desde el panel real, buscar un producto y explicar precio/stock. Si no conviene guardar cambios, mantenerlo en modo consulta."),
 ("2:00–4:00 · Compra de punta a punta", "Si la compra formal se interrumpió, repetir detalle → carrito → checkout con datos ficticios. Aclarar que el pago es simulado."),
 ("4:00–6:00 · Historial", "Abrir Mis compras y detalle para explicar número, total, estado y relación con Pedido/DetallePedido."),
 ("6:00–8:00 · Empleado o reportes (opcional)", "Mostrar solo si el login y la pantalla están preparados. El análisis reporta venta presencial y reportes; confirmar en vivo antes de prometer o depender de ellos."),
 ("8:00–10:00 · Preguntas", "Responder con una pantalla o archivo de código puntual. Si preguntan por seguridad, pago real o multiempresa, explicar brechas y plan de mejora."),
]
for a,b in demo: para(a,b)

heading("Preparación antes de exponer")
for item in [
 "Arrancar API, frontend y SQL Server; verificar que el catálogo cargue desde el navegador.",
 "Tener una sesión de cliente válida y datos de demostración; no improvisar credenciales.",
 "Comprobar el carrito y el historial previamente. No ingresar datos reales de tarjetas ni datos personales en pantalla.",
 "Abrir el código solo en los archivos mencionados si aparece una pregunta técnica.",
 "Si el checkout falla, mostrar catálogo, detalle, carrito e historial existentes. No afirmar que el cobro se completó.",
]: doc.add_paragraph(item,style='List Bullet')

heading("Fuentes utilizadas")
doc.add_paragraph("Requerimientos V2 F&R, páginas 3–14; análisis técnico y funcional de F&R adjunto; repositorio del proyecto: Codigo/FRFront y Codigo/FYR-API; capturas reales compartidas para catálogo, detalle, carrito, historial y gestión de productos; GYM_Presentacion.pptx como referencia de ritmo y estilo visual.")
doc.core_properties.title="Guion de exposición de F&R"
doc.core_properties.subject="Guion oral, matriz de contraste, requisitos, casos de uso y demostración"
doc.core_properties.author="F&R - Proyecto académico ICES"
doc.save(OUT_DOC)

print(OUT_PPT)
print(OUT_DOC)
