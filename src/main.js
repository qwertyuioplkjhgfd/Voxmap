const canvas = document.querySelector("canvas")
const gl = canvas.getContext("webgl2")

const debug = document.querySelector("div")

const texture = gl.createTexture()
const texture_png = new Image()

// look up where the vertex data needs to go.
let program
let handles

let timeSamples = Array(10).fill(0)
let then = 0
let time = 0
let running = false

main()

async function main(){

    texture_png.onload = () => {
        gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, true)
        gl.bindTexture(gl.TEXTURE_2D,texture)
        gl.texImage2D(gl.TEXTURE_2D,0,gl.RGBA,gl.RGBA,gl.UNSIGNED_BYTE,texture_png)
        gl.generateMipmap(gl.TEXTURE_2D)
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
    resize()
}

function prerender(){
    gl.activeTexture(gl.TEXTURE0)
    gl.bindTexture(gl.TEXTURE_2D,texture)
    gl.uniform1i(handles.textureSampler,0)
}
function render(now) {
    now *= 0.001 // convert to seconds
    const delta = Math.min(now - then, 0.1)

    timeSamples.shift()
    timeSamples.push(delta)
    debug.innerText = Math.round(10 * timeSamples.length / timeSamples.reduce((a, b) => a + b, 0)) / 10 + ' fps'

    time += delta
    then = now
    
    gl.viewport(0, 0, gl.canvas.width, gl.canvas.height)

    gl.uniform2f(handles.resolution, gl.canvas.width, gl.canvas.height)
    gl.uniform1f(handles.time, time)
    gl.uniform3f(handles.rotation, time, time, time)
    gl.uniform3f(handles.position, time, time, time+30)

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
    resizeCanvasToDisplaySize(gl.canvas, 1/5)
}

