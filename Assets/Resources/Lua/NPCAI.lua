-- 全局可热更的追击配置
changeDirInterval = 1
chaseDistance = 5 -- 距离<5米开始追击
randomDistance = 8 -- 距离>8米恢复随机
function CalcNPCMoveDir(currentPosX, currentPosZ, currentTime, lastChangeTime, currentDirX, currentDirZ, playerX, playerZ, distanceToPlayer)
    local needResetChangeTime = false

    if distanceToPlayer ~= nil and distanceToPlayer < chaseDistance then
        currentDirX = playerX - currentPosX
        currentDirZ = playerZ - currentPosZ
        needResetChangeTime = true
    else
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
    end

    return currentDirX, currentDirZ, needResetChangeTime
end