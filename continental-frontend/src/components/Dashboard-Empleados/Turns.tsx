import type { User } from "@/interfaces/User.interface"
import { ArrowRight } from "lucide-react"
import { useState, useEffect } from "react"

export const Turns = ({endBy, currenTurns, nextTurns}: {endBy: Date, currenTurns: User[], nextTurns: User[][]}) => {
  const [timeLeft, setTimeLeft] = useState('')

  useEffect(() => {
    const calculateTimeLeft = () => {
      const now = new Date().getTime()
      const targetTime = endBy.getTime()
      const difference = targetTime - now

      if (difference > 0) {
        const days = Math.floor(difference / (1000 * 60 * 60 * 24))
        const hours = Math.floor((difference % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60))
        const minutes = Math.floor((difference % (1000 * 60 * 60)) / (1000 * 60))
        const seconds = Math.floor((difference % (1000 * 60)) / 1000)

        if (days > 0) {
          setTimeLeft(`${days}d ${hours}h ${minutes}m ${seconds}s`)
        } else if (hours > 0) {
          setTimeLeft(`${hours}h ${minutes}m ${seconds}s`)
        } else if (minutes > 0) {
          setTimeLeft(`${minutes}m ${seconds}s`)
        } else {
          setTimeLeft(`${seconds}s`)
        }
      } else {
        setTimeLeft('Expirado')
      }
    }

    calculateTimeLeft()
    const timer = setInterval(calculateTimeLeft, 1000)

    return () => clearInterval(timer)
  }, [endBy])

  return (
    <div className="w-full flex items-center gap-4 h-full">
        <div className="flex flex-col gap-2 p-4 border border-continental-blue-dark rounded-lg">
            <h2 className="text-2xl font-bold mb-4 text-continental-blue-dark">Turno actual</h2>
            <div className="flex flex-col gap-2 h-full">
                {currenTurns.map((turn) => (
                    <div key={turn.username} className="flex flex-col gap-0 items-start justify-start hover:bg-continental-gray-4 p-2 rounded-lg cursor-pointer transition-colors">
                        <span className="font-bold">{turn.username}</span>
                        <span className="text-base text-continental-gray-1 uppercase">{turn.fullName}</span>
                    </div>
                ))}
                <div className="flex flex-col items-start ">
                    <span className="text-base">Termina en </span>
                    <span className="font-bold text-continental-gray-1">{timeLeft}</span>
                </div>
            </div>
        </div>
        <div className="flex flex-col gap-2 p-4 border border-continental-gray-1 rounded-lg flex-1 h-full">
            <div className="flex items-center gap-2 ">
                <h2 className="text-2xl font-bold  text-continental-gray-1">Siguientes turnos</h2>
                <ArrowRight  />
            </div>
            <div className="flex gap-8 h-full items-center">
                {nextTurns.map((turn, index) => (
                    <>
                        <div key={turn[0].username} className="flex flex-col gap-2">
                            {
                                turn.map((user) => (
                                    <div key={user.username} className="flex flex-col gap-0 items-start justify-start hover:bg-continental-gray-4 p-2 rounded-lg cursor-pointer transition-colors">
                                        <span className="font-bold">{user.username}</span>
                                        <span className="text-base text-continental-gray-1 uppercase">{user.fullName}</span>
                                    </div>
                                ))
                            }
                        </div>
                        {index < nextTurns.length - 1 && (
                            <ArrowRight className="text-continental-gray-1" size={20} />
                        )}
                    </>
                ))}
            </div>
        </div>
    </div>
  )
}
