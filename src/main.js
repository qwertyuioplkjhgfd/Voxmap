const debug = document.querySelector("div")
const joystick = document.querySelector("svg")
const canvas = document.querySelector("canvas")
const gl = canvas.getContext("webgl2")

const texture = gl.createTexture()
const texture_png = new Image()

const ns = 'http://www.w3.org/2000/svg'

// look up where the vertex data needs to go.
let program
let handles

const N = 30
let size = 100
let timeSamples = Array(30).fill(0)
let upSample = 4
let fps = 1
let then = 0
let time = 0
let running = false

const camera = {
    pos: { x: -200, y: 0, z: 3 },
    vel: { x: 0, y: 0, z: 0 },
    rot: { x: 0, y: 0, z: 0 },
}

main()

async function main(){

    texture_png.onload = () => {
        gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, true)
        gl.bindTexture(gl.TEXTURE_2D,texture)
        
        gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGB8UI,
                      gl.RGB_INTEGER, gl.UNSIGNED_BYTE, texture_png)
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.NEAREST);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.NEAREST);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
    }
    texture_png.src = "maps/texture.png"

    const vert = (await (await fetch("src/shader.vs")).text())
    .replace("#version 330 core", "#version 300 es")

    const frag = (await (await fetch("src/shader.fs")).text())
    .replace("#version 330 core", "#version 300 es")

    // setup GLSL program
    program = createProgramFromSources(gl, [vert, frag])

    handles = {
        position: gl.getAttribLocation(program, "vPosition"),
        coord: gl.getAttribLocation(program, "TexCoord"),
        resolution: gl.getUniformLocation(program, "iResolution"),
        time: gl.getUniformLocation(program, "iTime"),
        rotation: gl.getUniformLocation(program, "camRot"),
        position: gl.getUniformLocation(program, "camPos"),
    }

    const positionBuffer = gl.createBuffer();

    // Bind it to ARRAY_BUFFER (think of it as ARRAY_BUFFER = positionBuffer)
    gl.bindBuffer(gl.ARRAY_BUFFER, positionBuffer)

    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array([
        -1, -1, // first triangle
        +1, -1,
        -1, +1,
        -1, +1, // second triangle
        +1, -1,
        +1, +1,
    ]), gl.STATIC_DRAW)

    gl.vertexAttribPointer(
        handles.position,
        2, // 2 components per iteration
        gl.FLOAT, // the data is 32bit floats
        false, // don't normalize the data
        0, // 0 = move forward size * sizeof(type) each iteration to get the next position
        0, // start at the beginning of the buffer
    )

    gl.enable(gl.CULL_FACE)
    gl.cullFace(gl.BACK)

    gl.useProgram(program)
    gl.enableVertexAttribArray(handles.position)

    start()
    window.addEventListener('resize',resize)
<<<<<<< HEAD
    canvas.addEventListener('pointermove',(event)=>{
<<<<<<< HEAD
        camera.rot.z += event.movementX/size
        camera.rot.y += event.movementY/size
=======
        camera.rot.z += 3 * event.movementX/size
        camera.rot.x += 3 * event.movementY/size
>>>>>>> 0e15432... Fix camera rotation
=======
    document.addEventListener('click',()=>{
        canvas.requestPointerLock()
    })
    canvas.addEventListener('pointermove',(event)=>{
        controls.rot.z -= event.movementX/size
        controls.rot.x -= event.movementY/size
        controls.rot.x = Math.max(-0.2, Math.min(controls.rot.x, 0.2))
    })
    joystick.addEventListener('touchstart',()=>{
        controls.move.active = true
>>>>>>> 0fe8566... Limiting pitch, also nonfunctional hashing
    })
    joystick.addEventListener('pointermove',(event)=>{
        camera.vel.x = 10 * Math.pow(2*event.offsetX/size - 1, 3)
        camera.vel.y =-10 * Math.pow(2*event.offsetY/size - 1, 3)
    })
    window.addEventListener('keypress', (event) => {
        let wishdir = camera.rot.z
        switch (event.code) {
<<<<<<< HEAD
            case 'KeyW':
                wishdir += Math.PI/2
                break;
            case 'KeyA':
                wishdir += Math.PI
                break;
            case 'KeyS':
                wishdir += 3*Math.PI/2
                break;
            case 'KeyD':
=======
            case "KeyW":
            case "ArrowUp":
                controls.move.y = 1
                break;
            case "KeyS":
            case "ArrowDown":
                controls.move.y = -1
                break;
            case "KeyA":
            case "ArrowLeft":
                controls.move.x = -1
                break;
            case "KeyD":
            case "ArrowRight":
                controls.move.x = 1
                break;
        }
    })
    window.addEventListener('keyup', (event) => {
        switch (event.code) {
            case "KeyW":
            case "KeyS":
            case "ArrowUp":
            case "ArrowDown":
                controls.move.y = 0
                break;
            case "KeyA":
            case "KeyD":
            case "ArrowLeft":
            case "ArrowRight":
                controls.move.x = 0
>>>>>>> 0fe8566... Limiting pitch, also nonfunctional hashing
                break;
        }

        camera.vel.x += 10 * Math.cos(wishdir)
        camera.vel.y += 10 * Math.sin(wishdir)
    })
    setInterval(()=>{
        let target = 30
        upSample *= target/fps
        upSample = Math.max(1, Math.min(upSample,16))
        resize()
    },1000)

    resize()
}

function prerender(){
    gl.activeTexture(gl.TEXTURE0)
    gl.bindTexture(gl.TEXTURE_2D,texture)
    gl.uniform1i(handles.textureSampler,0)
}
function render(now) {
    now *= 0.001 // convert to seconds
    const delta = now - then

    timeSamples.shift()
    timeSamples.push(delta)
    fps = Math.round(N * timeSamples.length / timeSamples.reduce((a, b) => a + b, 0)) / N

    camera.pos.x += camera.vel.x*delta
    camera.pos.y += camera.vel.y*delta
    camera.pos.z += camera.vel.z*delta

    const rev = v => ( Math.sign(v) * Math.pow(Math.abs(v), 1/3) || 0 )
    joystick.lastElementChild.style.transform = 
        `translate(${rev(camera.vel.x)}vmin, ${-rev(camera.vel.y)}vmin)`

<<<<<<< HEAD
    debug.innerText = camera.vel.y.toPrecision(3) + 'm/s'
    + upSample.toPrecision(3) + ' x, ' 
    + fps.toPrecision(3) + ' fps' 
=======
    const num = x => x.toPrecision(3)
    const yrd = x => num(x/2)

    debug.innerText = `${num(fps)} fps, ${num(upSample)} upscaling
        position (m): ${yrd(camera.pos.x)}, ${yrd(camera.pos.y)}, ${yrd(camera.pos.z)}
        velocity (m/s): ${yrd(camera.vel.x)}, ${yrd(camera.vel.y)}, ${yrd(camera.vel.z)}
    `

    let sin = Math.sin(camera.rot.z)
    let cos = Math.cos(camera.rot.z)
    let ax = (controls.move.x * 100 * cos) - (controls.move.y * 100 * sin)
    let ay = (controls.move.x * 100 * sin) + (controls.move.y * 100 * cos)

    let drag = 1/8
    ax -= camera.vel.x/delta*drag
    ay -= camera.vel.y/delta*drag
    
    camera.vel.x += ax * delta
    camera.vel.y += ay * delta
>>>>>>> 0fe8566... Limiting pitch, also nonfunctional hashing

    camera.vel.x *= 0.2 ** delta
    camera.vel.y *= 0.2 ** delta
    camera.vel.z *= 0.2 ** delta

    time += delta
    then = now
    
    gl.viewport(0, 0, gl.canvas.width, gl.canvas.height)

    gl.uniform2f(handles.resolution, gl.canvas.width, gl.canvas.height)
    gl.uniform1f(handles.time, time)
    gl.uniform3f(handles.position, camera.pos.x, camera.pos.y, camera.pos.z)
    gl.uniform3f(handles.rotation, camera.rot.x, camera.rot.y, camera.rot.z)

    gl.drawArrays(gl.TRIANGLES, 0, 6)

    if(running) requestAnimationFrame(render)
}

function stop () {
    running = false
}
function start () {
    if(running) return
    running = true
    prerender()
    requestAnimationFrame(render)
}


function resize () {
    size = Math.min(window.innerWidth, window.innerHeight)
    resizeCanvasToDisplaySize(gl.canvas, 1/upSample)
}

