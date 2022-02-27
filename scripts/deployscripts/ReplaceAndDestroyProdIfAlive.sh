if python3 MinitwitAlive.py $1 $2; then
        echo staging droplet $1 alive 
        if python3 reassign_floating_ip.py $1 $2; then 
            echo reassign_floating_ip successful
            echo Destroying production droplet $3
            vagrant destroy -f $3
            if python3 rename_droplet.py $1 $3 $2; then
                echo rename of droplet $1 to $3 successful
            else 
                echo rename of droplet $1 failed
            fi
        else 
            echo staging droplet $1 not found
            cd ../../aspnet/
            vagrant destroy -f $3
        fi
else
    echo staging droplet $1 not alive
    cd ../../aspnet/
    vagrant destroy -f $3
fi