# 🎮 SISTEMA DE ENEMIGO Y COMBATE - INSTRUCCIONES DE CONFIGURACIÓN

## 📋 Scripts Creados

1. **EnemyAI.cs** - Inteligencia artificial del enemigo
2. **EnemyHealth.cs** - Sistema de vida y respawn del enemigo
3. **PlayerHealth.cs** - Sistema de vida del jugador
4. **movimiento.cs** (actualizado) - Sistema de ataque del jugador

---

## ⚙️ CONFIGURACIÓN DEL PLAYER

### 1. Agregar PlayerHealth al Player
1. Selecciona tu GameObject del Player en la jerarquía
2. En el Inspector, haz clic en "Add Component"
3. Busca y agrega "Player Health"
4. Configura los valores:
   - **Max Health**: 3 (toques que puede recibir)
   - **Invincibility Time**: 1 (segundos de invencibilidad tras recibir daño)
   - **Blink Interval**: 0.1 (velocidad del parpadeo)

### 2. Configurar Tag del Player
1. Con el Player seleccionado
2. En el Inspector, arriba, donde dice "Tag"
3. Selecciona "Player" (o créalo si no existe)
   - Para crear: Tag → Add Tag → + → Escribe "Player" → Save

### 3. Verificar AttackPoint
1. El AttackPoint debe estar configurado en el script "movimiento"
2. Si no existe, crea un Empty GameObject hijo del Player:
   - Click derecho en Player → Create Empty
   - Nómbralo "AttackPoint"
   - Colócalo frente al player (ajusta la posición X)
   - Asígnalo en el campo "Attack Point" del script movimiento

### 4. Configurar Layer de Enemigos
1. En Unity, ve a Edit → Project Settings → Tags and Layers
2. En "Layers", encuentra un slot vacío (ej: User Layer 6)
3. Escribe "Enemy"
4. Guarda los cambios

### 5. Configurar el script movimiento
En el Inspector del Player, en el componente "movimiento":
- **Attack Point**: Arrastra el AttackPoint que creaste
- **Attack Range**: 0.5 (ajusta según necesites)
- **Enemy Layers**: Selecciona la layer "Enemy"
- **Attack Damage**: 1 (mata de un golpe)
- **Attack Rate**: 2 (2 ataques por segundo)

---

## 🔴 CONFIGURACIÓN DEL ENEMIGO

### 1. Crear el GameObject del Enemigo
1. Crea un nuevo GameObject (botón derecho en Hierarchy → Create Empty)
2. Nómbralo "Enemy"
3. Agrega los siguientes componentes:
   - **Sprite Renderer** (arrastra tu sprite del enemigo)
   - **Rigidbody2D**:
     - Body Type: Dynamic
     - Gravity Scale: 3 (igual que el player)
     - Constraints: Freeze Rotation Z (marcado)
   - **Box Collider 2D** (o el collider que ya tengas)
     - Ajusta el tamaño al sprite

### 2. Configurar Tag y Layer del Enemigo
1. **Tag**: Selecciona "Enemy" (o créalo como hiciste con Player)
2. **Layer**: Selecciona "Enemy" (el que creaste anteriormente)

### 3. Agregar Scripts al Enemigo
Arrastra ambos scripts al enemigo:

#### A) EnemyAI
- **Move Speed**: 3 (velocidad de persecución)
- **Stopping Distance**: 1.5 (qué tan cerca se acerca al player)
- **Detection Range**: 10 (distancia desde donde detecta al player)
- **Damage To Player**: 1 (daño por toque)
- **Attack Cooldown**: 1 (segundos entre ataques)

#### B) EnemyHealth
- **Max Health**: 1 (muere de un golpe)
- **Respawn Time**: 3 (segundos para respawnear)

---

## 🎯 CAPAS Y COLISIONES (MUY IMPORTANTE)

### Configurar Collision Matrix
1. Ve a Edit → Project Settings → Physics 2D
2. En la Collision Matrix (matriz de colisiones):
   - ✅ **Player puede colisionar con Enemy** (debe estar marcado)
   - ✅ **Enemy puede colisionar con Ground** (debe estar marcado)
   - ✅ **Player puede colisionar con Ground** (debe estar marcado)

---

## 🎨 CONFIGURACIÓN DEL ANIMATOR (ATAQUE)

Si quieres que la animación de ataque funcione:

1. Abre el Animator Controller del Player
2. Crea un nuevo parámetro Float llamado "ataque"
3. Crea transiciones:
   - Idle → Ataque (condición: ataque > 0.5)
   - Ataque → Idle (condición: ataque < 0.5, Has Exit Time)

---

## 🧪 TESTING

### Prueba 1: Sistema de Vida del Player
1. Dale Play
2. Deja que el enemigo te toque 3 veces
3. Deberías ver:
   - Console muestra "Player recibió 1 de daño..."
   - Player parpadea después de cada golpe
   - Después del 3er golpe, escena se reinicia

### Prueba 2: Ataque del Player
1. Dale Play
2. Presiona la tecla de ataque (Enter/Return) cerca del enemigo
3. Deberías ver:
   - Console muestra "Golpeamos a Enemy"
   - Enemigo desaparece
   - Después de 3 segundos, enemigo reaparece en su posición inicial

### Prueba 3: Persecución del Enemigo
1. Dale Play
2. Aléjate del enemigo
3. Deberías ver:
   - Enemigo te sigue si estás dentro del Detection Range
   - Se detiene cuando está cerca (Stopping Distance)
   - Voltea su sprite según la dirección

---

## 🎮 CONTROLES

- **Movimiento**: A/D o Flechas Izq/Der
- **Salto**: Espacio
- **Ataque**: Enter/Return

---

## 🔧 AJUSTES COMUNES

### El enemigo no persigue al player:
- Verifica que el Player tenga el Tag "Player"
- Revisa que Detection Range sea suficientemente grande
- Asegúrate de que el enemigo tenga Rigidbody2D

### El ataque no daña al enemigo:
- Verifica que el enemigo tenga la Layer "Enemy"
- En el script movimiento del player, asegúrate de que Enemy Layers esté configurado
- Revisa que Attack Point esté asignado
- Aumenta Attack Range si es necesario

### El player muere de un solo golpe:
- Verifica que Max Health en PlayerHealth esté en 3
- Revisa que Attack Cooldown del enemigo esté en 1 segundo

### El enemigo no respawnea:
- Asegúrate de que el script EnemyHealth esté agregado
- Verifica que Respawn Time sea mayor a 0
- Revisa la consola por errores

---

## 📊 GIZMOS VISUALES (AYUDA PARA DEBUGGING)

En el Scene view, selecciona el Player o Enemy y verás:

### Player:
- **Círculo Cyan**: Ground Check
- **Círculo Rojo**: Rango de ataque

### Enemy:
- **Círculo Rojo**: Detection Range
- **Círculo Amarillo**: Stopping Distance

---

## 🎯 MEJORAS OPCIONALES

### Agregar sonidos:
```csharp
// En Attack():
AudioSource.PlayClipAtPoint(attackSound, transform.position);

// En TakeDamage():
AudioSource.PlayClipAtPoint(damageSound, transform.position);
```

### Agregar efectos de partículas:
```csharp
// En Attack():
Instantiate(hitEffect, attackPoint.position, Quaternion.identity);

// En Die():
Instantiate(deathEffect, transform.position, Quaternion.identity);
```

### Barra de vida UI:
Puedes crear un Canvas con un Slider para mostrar la vida del player:
```csharp
// En PlayerHealth Update():
healthSlider.value = currentHealth / (float)maxHealth;
```

---

## ❓ TROUBLESHOOTING

### Console muestra "NullReferenceException":
- Revisa que todos los campos públicos estén asignados en el Inspector
- Especialmente: AttackPoint, Player Tag, Layers

### Enemigo atraviesa el suelo:
- Asegúrate de que el suelo tenga un Collider2D
- Verifica la Collision Matrix (Enemy vs Ground)

### Player no puede atacar:
- Verifica que Attack Rate no sea demasiado alto
- Asegúrate de que Attack Point esté en frente del player
- Aumenta Attack Range temporalmente para probar

---

## 📝 NOTAS FINALES

- El sistema usa **Physics2D**, asegúrate de que todo sea 2D
- Los scripts son **modulares**, puedes ajustar valores sin tocar código
- El **respawn** mantiene la posición inicial del enemigo
- La **invencibilidad** evita que el player muera instantáneamente

¡Listo! Tu sistema de combate está completo 🎉
