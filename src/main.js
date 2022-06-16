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
    canvas.addEventListener('pointermove',(event)=>{
        camera.rot.z += event.movementX/size
        camera.rot.y += event.movementY/size
    })
    joystick.addEventListener('pointermove',(event)=>{
        camera.vel.x = 10 * Math.pow(2*event.offsetX/size - 1, 3)
        camera.vel.y =-10 * Math.pow(2*event.offsetY/size - 1, 3)
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

    debug.innerText = camera.vel.y.toPrecision(3) + 'm/s'
    + upSample.toPrecision(3) + ' x, ' 
    + fps.toPrecision(3) + ' fps' 

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

