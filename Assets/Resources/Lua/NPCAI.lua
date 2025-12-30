changeDirInterval = 0.5 -- 覆盖C#传的参数
function CalcNPCMoveDir(currentPosX, currentPosZ, currentTime, lastChangeTime, currentDirX, currentDirZ)
    local needResetChangeTime = false

    -- 碰边界换方向
    if currentPosX <= boundaryXMin or currentPosX >= boundaryXMax or 
       currentPosZ <= boundaryZMin or currentPosZ >= boundaryZMax then
        currentDirX = math.random(-100, 100)/100
        currentDirZ = math.random(-100, 100)/100
        needResetChangeTime = true
    end

    -- 定时换方向
    if currentTime - lastChangeTime >= changeDirInterval then
        currentDirX = math.random(-100, 100)/100
        currentDirZ = math.random(-100, 100)/100
        needResetChangeTime = true
    end

    return currentDirX, currentDirZ, needResetChangeTime
end